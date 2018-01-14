namespace TimeOff
open System.Collections.Generic
open TimeOff
open EventStorage

module Db = 
    open System
    open Logic

    let store = InMemoryStore.Create<UserId, RequestEvent>()

    let usersList = new List<Person>()
    usersList.Add(new Person (userId= 1, name = "Azedine" ))
    usersList.Add(new Person (userId= 2, name = "Loic" ))
    usersList.Add(new Person (userId= 3, name = "Andre" ))
    usersList.Add(new Person (userId= 3, name = "Bob" ))

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
        let r = result.[0]
        r

    let ValidateTimeOff request = 
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
        let r = result.[0]
        r
  
