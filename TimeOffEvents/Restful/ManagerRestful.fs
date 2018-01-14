﻿namespace TimeOff.Restful

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Http
open Suave.Successful
open TimeOff
open TimeOff.Repository
open JsonConvert


module Manager = 
    open Suave.RequestErrors
    open Suave.Filters

    let private getResourceFromReq<'a> (req : HttpRequest) =
        let getString rawForm = System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm |> getString |> fromJson<'a>

    let ManagerAPI (repository: ManagerRepository<RequestEvent>) =

        let resourcePath = "/Manager"
        let resourceIdPath = new PrintfFormat<(int -> string),unit,string,string,int>(resourcePath + "/%d")
        let badRequest = BAD_REQUEST "Resource not found"

        let handleResource requestError resource =
            match resource with
                | Some r -> r |> JSON
                | _ -> requestError

        let getAll _ = repository.GetAll () |> JSON

        choose [
           
            path (resourcePath+"/GetAll") >=> choose [
                GET >=> request (getResourceFromReq >> repository.GetAll >> JSON)
            ]
            path (resourcePath + "/Validate") >=> choose [
                POST >=> request (getResourceFromReq >> repository.ValidateTimeOff >> JSON)
            ] 
            path (resourcePath + "/Refuse") >=> choose [
                POST >=> request (getResourceFromReq >> repository.RefuseTimeOff >> JSON)
            ] 
            path (resourcePath + "/Cancel/Validate") >=> choose [
                POST >=> request (getResourceFromReq >> repository.ValidateCancelTimeOff >> JSON)
            ] 
            path (resourcePath + "/Cancel/Refuse") >=> choose [
                POST >=> request (getResourceFromReq >> repository.RefuseCancelTimeOff >> JSON)
            ] 
        ]