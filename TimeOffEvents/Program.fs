//module TimeOff.TestsRunner

//open Expecto

//[<EntryPoint>]
//let main args =
//  runTestsWithArgs { defaultConfig with ``parallel`` = false } args Tests.tests
  

namespace TimeOff
module TimeOff = 

    open System
    open Expecto
    open System.Collections.Generic
    open TimeOff
    open TimeOff.Repository
    open TimeOff.Restful
    open EventStorage
    open Logic


    open Suave
    open Suave.Web

    [<EntryPoint>]
    let main argv =

        let userRepository = {
            GetById = Db.getTimeOffByIdUser
            CreateTimeOff = Db.createTimeOffRequest
            CancelTimeOffByEmployee = Db.cancelTimeOffByEmployee
            GetCurrentUserById = Db.getUserById
            GetCurrentBalanceById = Db.getBalanceById
        }

        let managerRepository = {
            GetAll = Db.getAllTimeOffRequest          
            ValidateTimeOff = Db.validateTimeOff
            RefuseTimeOff = Db.refuseTimeOff           
            ValidateCancelTimeOff = Db.validateCancelTimeOff
            RefuseCancelTimeOff = Db.refuseCancelTimeOff
        }

        let app =
            choose [
                User.UserAPI userRepository
                Manager.ManagerAPI managerRepository
                RequestErrors.NOT_FOUND "Found no handlers"
            ]

        startWebServer defaultConfig app

        0

