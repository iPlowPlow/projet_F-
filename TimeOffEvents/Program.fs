module TimeOff.TestsRunner

open System
open Expecto
open System.Collections.Generic
open Logic
open EventStorage

let mutable continueProg = true;
let usersList = new List<User>()
let store = InMemoryStore.Create<UserId, RequestEvent>()


for i = 1 to 4 do
    usersList.Add(User.Employee i);

let manager = User.Manager;

let demandeConges() = 
    printfn "Faire une demande de conges"
    printfn "Merci de renseigner les champs suivants : \" idUser, dateStart, dateEnd\" "
    
    let mutable testExist = false;
    let mutable nbTry = 0;
    printfn "idUser : ";
    let mutable idUser = 0;
    while testExist = false && nbTry <3  do
        idUser <- System.Console.ReadLine() |> int;
        for user in usersList do
            if user = User.Employee idUser then 
                testExist <- true;
        if testExist = false then printfn "Utilisateur non reconu, merci de recommencer";
        nbTry <- nbTry + 1;
    
    if testExist = false  then printfn "Trop de tentatives, retour au menu"
    else 

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

        let command = Command.RequestTimeOff { UserId = idUser
                                               RequestId = new Guid();
                                               Start = { Date = DateTime(dateStartY, dateStartM, dateStartJ); HalfDay = AM }
                                               End = { Date = DateTime(dateEndY, dateEndM, dateEndJ); HalfDay = PM }}
        //Return RequestCreated
        let result = Logic.handleCommand store command

        printfn "Fin de la demande de conges"


let listeConges() = 
    printfn "liste des demandes de conges"
    let mutable testExist = false;
    let mutable nbTry = 0;
    printfn "idUser : ";
    let mutable idUser = 0;
    while testExist = false && nbTry <3  do
        idUser <- System.Console.ReadLine() |> int;
        for user in usersList do
            if user = User.Employee idUser then 
               testExist <- true;
        if testExist = false then printfn "Utilisateur non reconu, merci de recommencer";
        nbTry <- nbTry + 1;
    
    if testExist = false  then printfn "Trop de tentatives, retour au menu"
    //else
    //   store.GetStream.


let main() =
    let mutable i = 0;  
    while continueProg do
        
        printfn "Bienvenu dans le gestionnaire de conges. Merci de choisir une fonctionnalite.";
        printfn "1 : Faire une demande de congés";
        printfn "2 : liste des demandes de congés";
        printfn "0 : Quitter";

        let choice =  System.Console.ReadLine();
        if choice = "1" then demandeConges();
        elif choice = "2" then listeConges();
        elif choice = "0" then continueProg <- false;
        else  printfn "Ce choix n'existe pas";
        
        printfn "Fin de l'action";


main();



