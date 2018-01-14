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

type Person(name:string, userId:UserId) =
    member x.Name = name
    member x.UserId = userId

type TimeOffRequest = {
    UserId: UserId
    RequestId: Guid
    Start: Boundary
    End: Boundary
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
    | RequestCreated of TimeOffRequest 
    | RequestValidated of TimeOffRequest 
    | RequestRefused of TimeOffRequest 
    | RequestCancelCreated of TimeOffRequest 
    | RequestCancelValidatedByEmployee of TimeOffRequest 
    | RequestCancelValidated of TimeOffRequest 
    | RequestCancelRefused of TimeOffRequest with
    member this.Request =
        match this with
        | RequestCreated request -> request 
        | RequestValidated request -> request
        | RequestRefused request -> request
        | RequestCancelCreated request -> request
        | RequestCancelValidatedByEmployee request -> request
        | RequestCancelValidated request -> request
        | RequestCancelRefused request -> request

       


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
        | RequestCreated request -> PendingValidation request
        | RequestValidated request -> Validated request
        | RequestRefused request -> Refused request
        | RequestCancelCreated request -> CancelPendingValidation request
        | RequestCancelValidatedByEmployee request -> CancelValidatedByEmployee request
        | RequestCancelValidated request -> CancelValidated request
        | RequestCancelRefused request -> CancelRefused request
      

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
            Ok [RequestCreated request]

    

    let cancelRequestByEmployee requestState date =
        if date > DateTime.Today then
            match requestState with
                | PendingValidation request ->
                    Ok [RequestCancelValidatedByEmployee request]
                | Validated request ->
                    Ok [RequestCancelValidatedByEmployee request]
                | _ ->
                    Error "Request cannot be validated"
        else 
            match requestState with
                | PendingValidation request ->
                    Ok [RequestCancelCreated request]
                | Validated request ->
                    Ok [RequestCancelCreated request]
                | _ ->
                    Error "Request cannot be validated"


    let validateCancelRequest requestState =
        match requestState with
        | CancelPendingValidation request ->
            Ok [RequestCancelValidated request]
        | Validated request ->
            Ok [RequestCancelRefused request]
        | PendingValidation request ->
            Ok [RequestCancelRefused request]
        | CancelRefused request ->
            Ok [RequestCancelRefused request]
        | _ ->
            Error "Cancel Request cannot be validate"

    let refuseCancelRequest requestState =
        match requestState with
        | CancelPendingValidation request ->
            Ok [RequestCancelRefused request]
        | _ ->
            Error "Cancel Request cannot be refused"

    let validateRequest requestState =
        match requestState with
        | PendingValidation request ->
            Ok [RequestValidated request]
        | _ ->
            Error "Request cannot be validated"
    let refuseRequest requestState =
        match requestState with
        | PendingValidation request ->
            Ok [RequestRefused request]
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