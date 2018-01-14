namespace TimeOff

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Http
open Suave.Successful
open TimeOff
open JsonConvert


module Restful = 
    open Suave.RequestErrors
    open Suave.Filters

    let private getResourceFromReq<'a> (req : HttpRequest) =
        let getString rawForm = System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm |> getString |> fromJson<'a>

    let TimeOffWebPart (repository: Repository<RequestEvent>) =
        let badRequest = BAD_REQUEST "Resource not found"

        let handleResource requestError resource =
            match resource with
                | Some r -> r |> JSON
                | _ -> requestError

        let getAll _ = repository.GetAll () |> JSON

        choose [
            path "/TimeOff/Create" >=> choose [
                POST >=> request (getResourceFromReq >> repository.CreateTimeOff >> JSON)
            ] 
            path "/TimeOff/Get" >=> choose [
                GET >=> request (getResourceFromReq >> repository.GetAll >> JSON)
            ] 
        ]
