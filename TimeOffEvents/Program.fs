module TimeOff.TestsRunner

open System
open Expecto
open System.Collections.Generic

let mutable continueProg = true;
let TimeOffRequestList = new List<TimeOffRequest>()

let demandeConges() = 
     printfn "Faire une demande de conges"
     printfn "Merci de renseigner les champs suivants : \" idUser, dateStart, dateEnd\" "
     printfn "idUser : ";
     let idUser = System.Console.ReadLine() |> int;
     printfn "dateStart : "
     printfn "Jour : "
     let dateStartJ = System.Console.ReadLine() |> int; 
     printfn "Mois : "
     let dateStartM = System.Console.ReadLine() |> int;
     printfn "Annee : "
     let dateStartY = System.Console.ReadLine() |> int;
     printf "dateEnd : "
     printfn "Jour : "
     let dateEndJ = System.Console.ReadLine() |> int;
     printfn "Mois : "
     let dateEndM = System.Console.ReadLine() |> int;
     printfn "Annee : "
     let dateEndY = System.Console.ReadLine() |> int;

     let timeOff = {
        UserId = idUser
        RequestId = new Guid();
        Start = { Date = DateTime(dateStartY, dateStartM, dateStartJ); HalfDay = AM }
        End = { Date = DateTime(dateEndY, dateEndM, dateEndJ); HalfDay = PM } 
     }

     let commande = Command.RequestTimeOff timeOff;

     RequestEvent.RequestCreated timeOff;

     TimeOffRequestList.Add(timeOff);



let listeConges() = 
     printfn "liste des demandes de conges"
    



let main() =
    let mutable i = 0;  
    while continueProg do
        
        printfn "Bienvenu dans le gestionnaire de conges. Merci de choisir une fonctionnalite.";
        printfn "1 : Faire une demande de congés";
        printfn "2 : liste des demandes de congés";
        printfn "0 : Quitter";

        let choice =  System.Console.ReadLine();
        if choice = "1" then demandeConges();
        elif choice = "0" then continueProg <- false;
        else  printfn "Ce choix n'existe pas";

    


main();