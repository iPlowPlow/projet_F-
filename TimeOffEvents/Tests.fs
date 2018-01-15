module TimeOff.Tests

open Expecto
open EventStorage

let Given events = events
let When command events = events, command
let Then expected message (events: RequestEvent list, command) =
    let store = InMemoryStore.Create<UserId, RequestEvent>()
    for event in events do
      let stream = store.GetStream event.Request.UserId
      stream.Append [event]
    let result = Logic.handleCommand store command
    Expect.equal result expected message

open System
open TimeOff.Logic

let creationTests =
  testList "Creation tests" [
    test "A request is created" {
      let request = {
        UserId = 1
        RequestId = Guid.Empty
        Start = { Date = DateTime.Now; HalfDay = AM }
        End = { Date = DateTime(2018,01,10); HalfDay = PM } }
      let myDate = {DateCreationEvent= DateTime.Now} 
      Given [ ]
      |> When (RequestTimeOff request)
      |> Then (Ok [RequestCreated (request,myDate)]) "The request has been created"
    }
  ]

let validationTests =
  testList "Validation tests" [
    test "A request is validated" {
      let request = {
        UserId = 1
        RequestId = Guid.Empty
        Start = { Date = DateTime.Now; HalfDay = AM }
        End = { Date = DateTime(2018,01,10); HalfDay = PM } }
      let myDate = {DateCreationEvent= DateTime.Now} 
      Given [ RequestCreated (request,myDate)]
      |> When (ValidateRequest (1, Guid.Empty))
      |> Then (Ok [RequestValidated (request,myDate)]) "The request has been validated"
    }
  ]


let refuseTests =
  testList "Refuse tests" [
    test "A request is refused" {
      let request = {
        UserId = 1
        RequestId = Guid.Empty
        Start = { Date = DateTime.Now; HalfDay = AM }
        End = { Date = DateTime(2018,01,10); HalfDay = PM } }
      let myDate = {DateCreationEvent= DateTime.Now} 
      Given [ RequestCreated (request,myDate)]
      |> When (RefuseRequest (1, Guid.Empty))
      |> Then (Ok [RequestRefused (request,myDate)]) "The request has been refuse"
    }
  ]
  
//Test que si  un employee fait une demande d'annulation pour un congés dans le passé créer un request pour le manageur
let cancelRequestCreatedTests =
  testList "cancel Request tests" [
    test "A cancel request is created" {
      let request = {
        UserId = 1
        RequestId = Guid.Empty
        Start = { Date = DateTime(2018,01,01); HalfDay = AM }
        End = { Date = DateTime(2018,01,10); HalfDay = PM } }
      let myDate = {DateCreationEvent= DateTime.Now} 
      Given [ RequestValidated (request,myDate)]
      |> When (CancelTimeOffByEmployeeRequest (1, Guid.Empty))
      |> Then (Ok [RequestCancelCreated (request,myDate)]) "The request has been validated"
    }
  ]

//Test que si un employee fait une demande d'annulation pour un congés dans le futur elle est auto validée
let cancelRequestAutoValidateTests =
  testList "cancel Request tests" [
    test "A cancel request is created" {
      let request = {
        UserId = 1
        RequestId = Guid.Empty
        Start = { Date = DateTime(2018,01,20); HalfDay = AM }
        End = { Date = DateTime(2018,01,25); HalfDay = PM } }
      let myDate = {DateCreationEvent= DateTime.Now} 
      Given [ RequestValidated (request,myDate)]
      |> When (CancelTimeOffByEmployeeRequest (1, Guid.Empty))
      |> Then (Ok [RequestCancelValidated (request,myDate)]) "The request has been validated"
    }
  ]

//Test qu'un manageur peut valider une demande de cancel
let cancelRequestValidateTests =
  testList "cancel Request tests" [
    test "A cancel request is created" {
      let request = {
        UserId = 1
        RequestId = Guid.Empty
        Start = { Date = DateTime(2018,01,20); HalfDay = AM }
        End = { Date = DateTime(2018,01,25); HalfDay = PM } }
      let myDate = {DateCreationEvent= DateTime.Now} 
      Given [RequestCancelCreated (request,myDate)]
      |> When (ValidateCancelRequest (1, Guid.Empty))
      |> Then (Ok [RequestCancelValidated (request,myDate)]) "The cancel request has been validated"
    }
  ]

let cancelRequestRefuseTests =
  testList "cancel Request tests" [
    test "A cancel request is created" {
      let request = {
        UserId = 1
        RequestId = Guid.Empty
        Start = { Date = DateTime(2018,01,20); HalfDay = AM }
        End = { Date = DateTime(2018,01,25); HalfDay = PM } }
      let myDate = {DateCreationEvent= DateTime.Now} 
      Given [ RequestCancelCreated (request,myDate)]
      |> When (RefuseCancelRequest (1, Guid.Empty))
      |> Then (Ok [RequestCancelRefused (request,myDate)]) "The cancel request has been refuse"
    }
  ]

let ShouldGetCorrectBalance =
  testList "Get Balance tests" [
    test "A request is validated" {
      let userId = 1
      let request = {
        UserId = userId
        RequestId = Guid.Empty
        Start = { Date = DateTime(2018,02,1); HalfDay = AM }
        End = { Date = DateTime(2018,02,10); HalfDay = PM } }
      let myDate = {DateCreationEvent= DateTime.Now} 
      let expected:double = (double)10.0
      Expect.equal expected (Db.getBalanceById(userId).Value).Available "The balance is correctly found"
    }
  ]

let tests =
  testList "All tests" [
    creationTests
    validationTests
    refuseTests
    cancelRequestCreatedTests
    cancelRequestAutoValidateTests
    cancelRequestValidateTests
    cancelRequestRefuseTests
    ShouldGetCorrectBalance
  ]