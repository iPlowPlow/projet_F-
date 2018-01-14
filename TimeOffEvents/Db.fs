namespace TimeOff
open System.Collections.Generic
open TimeOff
open EventStorage

module Db = 
    open System
    open Logic

    let store = InMemoryStore.Create<UserId, RequestEvent>()

    let usersList = new List<Person>()
    usersList.Add(new Person (userId= 1, name = "Azedine", email="az@gmail.com" ))
    usersList.Add(new Person (userId= 2, name = "Loic", email="loic@gmail.com" ))
    usersList.Add(new Person (userId= 3, name = "Andre", email="andre@gmail.com" ))
    usersList.Add(new Person (userId= 4, name = "Bob", email="bob@gmail.com" ))

    let manager = User.Manager;

    let getAllTimeOffRequest ()=
        let seqEvents = seq {
            for user in usersList do
                let stream = store.GetStream user.UserId
                let listEventsUser = stream.ReadAll();
                if Seq.length listEventsUser > 0 then 
                     for event in listEventsUser do 
                        yield(event)       
        }
       
        seqEvents


    let createTimeOffRequest request =

        let newRequest = { UserId = request.UserId
                           RequestId = Guid.NewGuid();
                           Start = request.Start
                           End = request.End
                         }
        let seqEvents = seq {
            let command = Command.RequestTimeOff newRequest
            let result = Logic.handleCommand store command
            match result with
                | Ok events ->
                    for event in events do
                        let stream = store.GetStream event.Request.UserId
                        stream.Append [event]
                        if event.Request.RequestId = newRequest.RequestId then yield event
                | Error e -> printfn "Error: %s" e
        }
        let result = Seq.toArray seqEvents
        if result.Length > 0 then
            let r = result.[0]
            Some r
        else 
            None

    let validateTimeOff request = 
        let seqEvents = seq {
            let command = Command.ValidateRequest (request.UserId, request.RequestId)
            let result = Logic.handleCommand store command
            match result with
                | Ok events ->
                    for event in events do
                        let stream = store.GetStream event.Request.UserId
                        stream.Append [event]
                        if event.Request.RequestId = request.RequestId then yield event
                | Error e -> printfn "Error: %s" e
        }

        let result = Seq.toArray seqEvents
        if result.Length > 0 then
            let r = result.[0]
            Some r
        else 
            None
  

    let refuseTimeOff request = 
        let seqEvents = seq {
            let command = Command.RefuseRequest (request.UserId, request.RequestId)
            let result = Logic.handleCommand store command
            match result with
                | Ok events ->
                    for event in events do
                        let stream = store.GetStream event.Request.UserId
                        stream.Append [event]
                        if event.Request.RequestId = request.RequestId then yield event
                | Error e -> printfn "Error: %s" e
        }

        let result = Seq.toArray seqEvents
        if result.Length > 0 then
            let r = result.[0]
            Some r
        else 
            None

    let cancelTimeOffByEmployee request = 
        let seqEvents = seq {
            let command = Command.CancelTimeOffByEmployeeRequest (request.UserId, request.RequestId)
            let result = Logic.handleCommand store command
            match result with
                | Ok events ->
                    for event in events do
                        let stream = store.GetStream event.Request.UserId
                        stream.Append [event]
                        if event.Request.RequestId = request.RequestId then yield event
                | Error e -> printfn "Error: %s" e
        }

        let result = Seq.toArray seqEvents
        if result.Length > 0 then
            let r = result.[0]
            Some r
        else 
            None

    let validateCancelTimeOff request = 
        let seqEvents = seq {
            let command = Command.ValidateCancelRequest (request.UserId, request.RequestId)
            let result = Logic.handleCommand store command
            match result with
                | Ok events ->
                    for event in events do
                        let stream = store.GetStream event.Request.UserId
                        stream.Append [event]
                        if event.Request.RequestId = request.RequestId then yield event
                | Error e -> printfn "Error: %s" e
        }

        let result = Seq.toArray seqEvents
        if result.Length > 0 then
            let r = result.[0]
            Some r
        else 
            None

    let refuseCancelTimeOff request = 
        let seqEvents = seq {
            let command = Command.RefuseCancelRequest (request.UserId, request.RequestId)
            let result = Logic.handleCommand store command
            match result with
                | Ok events ->
                    for event in events do
                        let stream = store.GetStream event.Request.UserId
                        stream.Append [event]
                        if event.Request.RequestId = request.RequestId then yield event
                | Error e -> printfn "Error: %s" e
        }

        let result = Seq.toArray seqEvents
        if result.Length > 0 then
            let r = result.[0]
            Some r
        else 
            None

    let getTimeOffByIdUser id =
        let stream = store.GetStream id
        let listEventsUser = stream.ReadAll();

        Some listEventsUser

    let getUserById id =
        let isUserId number (elem:Person) = elem.UserId.Equals(number) = true
        let mutable person:Person = usersList.[0]; 
        for user in usersList do
            if(user.UserId.Equals(id)) then person <- user
        Some person

    let getBalanceById id =
        let stream = store.GetStream id
        let listEventsUser = stream.ReadAll();
        let userRequests = getAllRequests listEventsUser
        let mutable used:int = 0; 
        let mutable planned:int = 0; 
        for i in userRequests do
            let currentRequest = i.Value.Request
            let mutable TimeOffLength:int = ((int)((currentRequest.End.Date - currentRequest.Start.Date).TotalDays)+1)*2
            if(currentRequest.Start.HalfDay.Equals(HalfDay.PM)) then 
                TimeOffLength<- (TimeOffLength-1)
            if(currentRequest.End.HalfDay.Equals(HalfDay.AM)) then 
                TimeOffLength<- (TimeOffLength-1)
            if(currentRequest.Start.Date > DateTime.Now) then
                planned <- planned + TimeOffLength else
                    used <- used + TimeOffLength

        let usedDays:double = ((double)used)/2.0;
        let plannedDays:double = ((double)planned)/2.0;
        let availableDays:double = 20.0 - (usedDays+plannedDays)
        let newSolde = new SoldeJour(userId=id,total=20.0,used=usedDays,planned=plannedDays,available=availableDays);

        Some newSolde
