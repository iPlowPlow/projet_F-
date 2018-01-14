namespace TimeOff

open System
open EventStorage
open System.Collections.Generic

type User =
    | Employee of int
    | Manager

type HalfDay = | AM | PM

type Boundary = {
    Date: DateTime
    HalfDay: HalfDay
}

type UserId = int

type DateEvent = {
    DateCreationEvent: DateTime
}

type Person(name:string, userId:UserId, email:string) =
    member x.Name = name
    member x.UserId = userId
    member x.Email = email

type SoldeJour(userId: int, total: double, used: double, planned: double, available: double) =
    member x.UserId = userId
    member x.Total = total
    member x.Used = used
    member x.Planned = planned
    member x.Available = available 

type TimeOffRequest = {
    UserId: UserId
    RequestId: Guid
    Start: Boundary
    End: Boundary
}

type RequestPull = {
    item1: TimeOffRequest
    item2: DateEvent
}

type Command =
    | RequestTimeOff of TimeOffRequest
    | ValidateRequest of UserId * Guid 
    | RefuseRequest of UserId * Guid
    | RequestCancelTimeOffByEmployee of UserId * Guid
    | ValidateCancelRequest of UserId * Guid 
    | RefuseCancelRequest of UserId * Guid 
    member this.UserId =
        match this with
        | RequestTimeOff request -> request.UserId
        | ValidateRequest (userId, _) -> userId
        | RefuseRequest  (userId, _) -> userId
        | RequestCancelTimeOffByEmployee  (userId, _) -> userId
        | ValidateCancelRequest  (userId, _) -> userId
        | RefuseCancelRequest  (userId, _) -> userId


type RequestEvent =
    | RequestCreated of TimeOffRequest*DateEvent
    | RequestValidated of TimeOffRequest*DateEvent
    | RequestRefused of TimeOffRequest*DateEvent
    | RequestCancelCreated of TimeOffRequest*DateEvent 
    | RequestCancelValidatedByEmployee of TimeOffRequest*DateEvent 
    | RequestCancelValidated of TimeOffRequest*DateEvent 
    | RequestCancelRefused of TimeOffRequest*DateEvent with
    member this.Request =
        match this with
        | RequestCreated (request, date) -> request 
        | RequestValidated (request, date) -> request
        | RequestRefused (request, date) -> request
        | RequestCancelCreated (request, date) -> request
        | RequestCancelValidatedByEmployee (request, date) -> request
        | RequestCancelValidated (request, date) -> request
        | RequestCancelRefused (request, date) -> request
    member this.Date =
        match this with
        | RequestCreated (request, date) -> date 
        | RequestValidated (request, date) -> date
        | RequestRefused (request, date) -> date
        | RequestCancelCreated (request, date) -> date
        | RequestCancelValidatedByEmployee (request, date) -> date
        | RequestCancelValidated (request, date) -> date
        | RequestCancelRefused (request, date) -> date
   



module Logic =

    type RequestState =
        | NotCreated
        | PendingValidation of TimeOffRequest
        | Validated of TimeOffRequest 
        | Refused of TimeOffRequest
        | CancelPendingValidation of TimeOffRequest 
        | CancelValidatedByEmployee of TimeOffRequest  
        | CancelValidated of TimeOffRequest 
        | CancelRefused of TimeOffRequest with
        member this.Request =
            match this with
            | NotCreated -> invalidOp "Not created"
            | PendingValidation request
            | Validated request -> request
            | Refused request -> request
            | CancelPendingValidation request -> request
            | CancelValidatedByEmployee request -> request
            | CancelValidated request -> request
            | CancelRefused request -> request
        member this.IsActive =
            match this with
            | NotCreated -> false
            | PendingValidation _
            | Validated _ -> true
            | Refused _ -> true
            | CancelPendingValidation _ -> true
            | CancelValidatedByEmployee _ -> true
            | CancelValidated _ -> true
            | CancelRefused _ -> true


    let evolve _ event =
        match event with
        | RequestCreated (request, date) -> PendingValidation request
        | RequestValidated (request, date) -> Validated request
        | RequestRefused (request, date) -> Refused request
        | RequestCancelCreated (request, date) -> CancelPendingValidation request
        | RequestCancelValidatedByEmployee (request, date) -> CancelValidatedByEmployee request
        | RequestCancelValidated (request, date) -> CancelValidated request
        | RequestCancelRefused (request, date) -> CancelRefused request
      

    let getRequestState events =
        events |> Seq.fold evolve NotCreated

    let getAllRequests events =
        let folder requests (event: RequestEvent) =
            let state = defaultArg (Map.tryFind event.Request.RequestId requests) NotCreated
            let newState = evolve state event
            requests.Add (event.Request.RequestId, newState)

        events |> Seq.fold folder Map.empty

    let overlapWithAnyRequest (previousRequests: TimeOffRequest seq) request =
        false //TODO

    let createRequest previousRequests request =
        if overlapWithAnyRequest previousRequests request then
            Error "Overlapping request"
        elif request.Start.Date <= DateTime.Today then
            Error "The request starts in the past"
        else
            let myDate = {DateCreationEvent= DateTime.Now} 
            Ok [RequestCreated (request,myDate) ]

    

    let cancelRequestByEmployee requestState date =
        let myDate = {DateCreationEvent= DateTime.Now} 
        if date > DateTime.Today then
            match requestState with
                | PendingValidation request ->
                    Ok [RequestCancelValidatedByEmployee (request,myDate)]
                | Validated request ->
                    Ok [RequestCancelValidatedByEmployee (request,myDate)]
                | _ ->
                    Error "Request cannot be validated"
        else 
            match requestState with
                | PendingValidation request ->
                    Ok [RequestCancelCreated (request,myDate)]
                | Validated request ->
                    Ok [RequestCancelCreated (request,myDate)]
                | _ ->
                    Error "Request cannot be validated"


    let validateCancelRequest requestState =
        let myDate = {DateCreationEvent= DateTime.Now} 
        match requestState with
        | CancelPendingValidation request ->
            Ok [RequestCancelValidated (request,myDate)]
        | Validated request ->
            Ok [RequestCancelRefused (request,myDate)]
        | PendingValidation request ->
            Ok [RequestCancelRefused (request,myDate)]
        | CancelRefused request ->
            Ok [RequestCancelRefused (request,myDate)]
        | _ ->
            Error "Cancel Request cannot be validate"

    let refuseCancelRequest requestState =
        let myDate = {DateCreationEvent= DateTime.Now} 
        match requestState with
        | CancelPendingValidation request ->
            Ok [RequestCancelRefused (request,myDate)]
        | _ ->
            Error "Cancel Request cannot be refused"

    let validateRequest requestState =
        let myDate = {DateCreationEvent= DateTime.Now} 
        match requestState with
        | PendingValidation request ->
            Ok [RequestValidated (request,myDate)]
        | _ ->
            Error "Request cannot be validated"

    let refuseRequest requestState =
        let myDate = {DateCreationEvent= DateTime.Now} 
        match requestState with
        | PendingValidation request ->
            Ok [RequestRefused (request,myDate)]
        | _ ->
            Error "Request cannot be refused"

    let handleCommand (store: IStore<UserId, RequestEvent>) (command: Command) =
        let userId = command.UserId
        let stream = store.GetStream userId
        let events = stream.ReadAll()
        let userRequests = getAllRequests events

        match command with
        | RequestTimeOff request ->
            let activeRequests =
                userRequests
                |> Map.toSeq
                |> Seq.map (fun (_, state) -> state)
                |> Seq.where (fun state -> state.IsActive)
                |> Seq.map (fun state -> state.Request)

            createRequest activeRequests request

        | ValidateRequest (_, requestId) ->
            let requestState = defaultArg (userRequests.TryFind requestId) NotCreated
            validateRequest requestState
        | RefuseRequest (_, requestId) ->
            let requestState = defaultArg (userRequests.TryFind requestId) NotCreated
            refuseRequest requestState
        | RequestCancelTimeOffByEmployee (_, requestId) ->
            let requestState = defaultArg (userRequests.TryFind requestId) NotCreated
            cancelRequestByEmployee requestState requestState.Request.Start.Date
        | ValidateCancelRequest (_, requestId) ->
            let requestState = defaultArg (userRequests.TryFind requestId) NotCreated
            validateCancelRequest requestState
        | RefuseCancelRequest (_, requestId) ->
            let requestState = defaultArg (userRequests.TryFind requestId) NotCreated
            refuseCancelRequest requestState

       

    let getHalfDayString halfDay =
        if (halfDay.Equals(AM)) then "AM" else "PM"

    let getUserName (userId:int, userList:List<Person>) = 
        let mutable returnName:string = String.Empty; 
        for i in userList do
            if(userId.Equals(i.UserId)) then returnName <- i.Name
        returnName