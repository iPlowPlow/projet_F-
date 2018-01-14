namespace TimeOff.Restful

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Http
open Suave.Successful
open TimeOff
open TimeOff.Repository
open JsonConvert


module User = 
    open Suave.RequestErrors
    open Suave.Filters

    let private getResourceFromReq<'a> (req : HttpRequest) =
        let getString rawForm = System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm |> getString |> fromJson<'a>

    let UserAPI (repository: UserRepository<RequestEvent>) =

        let resourcePath = "/User"
        let resourceIdPath = new PrintfFormat<(int -> string),unit,string,string,int>(resourcePath + "/%d")
        let ressourceIdPathTimeOff = new PrintfFormat<(int -> string),unit,string,string,int>(resourcePath + "/GetTimeOffByIdUser/%d")
        let badRequest = BAD_REQUEST "Resource not found"

        let handleResource requestError resource =
            match resource with
                | Some r -> r |> JSON
                | _ -> requestError

        let getResourceById =
            repository.GetById >> handleResource (NOT_FOUND "Resource not found")
       
        choose [
           
            path (resourcePath+"/Create") >=> choose [
                POST >=> request (getResourceFromReq >> repository.CreateTimeOff >> JSON)
            ] 
            path (resourcePath + "/Cancel/Employee") >=> choose [
                POST >=> request (getResourceFromReq >> repository.CancelTimeOffByEmployee >> JSON)
            ]
            
            GET >=> pathScan ressourceIdPathTimeOff getResourceById
        ]
