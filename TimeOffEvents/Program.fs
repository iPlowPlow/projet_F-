module TimeOff.TestsRunner

open System
open Expecto
open System.Collections.Generic
open Logic
open EventStorage

let mutable continueProg = true;
let store = InMemoryStore.Create<UserId, RequestEvent>()

let usersList = new List<Person>()
usersList.Add(new Person (userId= 1, name = "Azedine" ))
usersList.Add(new Person (userId= 2, name = "Loic" ))
usersList.Add(new Person (userId= 3, name = "Andre" ))
usersList.Add(new Person (userId= 3, name = "Bob" ))

(*for i = 1 to 4 do
    usersList.Add(User.Employee i);*)

let manager = User.Manager;

let demandeConges() = 
    printfn "==========================Faire une demande de conges====================="
    printfn "Merci de renseigner les champs suivants : \" idUser, dateStart, dateEnd\" "
    
    let mutable testExist = false;
    let mutable nbTry = 0;
    printfn "idUser : ";
    let mutable idUser = 0;
    while testExist = false && nbTry <3  do
        idUser <- System.Console.ReadLine() |> int;
        for user in usersList do
            if user.UserId = int idUser then 
                testExist <- true;
        if testExist = false then printfn "Utilisateur non reconu, merci de recommencer";
        nbTry <- nbTry + 1;
    
    if testExist = false  then printfn "Trop de tentatives, retour au menu"
    else 
        printfn "Utilisateur selectionne: %s" (string (Logic.getUserName(idUser, usersList)))
        printfn "=============================== dateStart : ==================================="
        printfn "Jour : "
        let dateStartJ = System.Console.ReadLine() |> int; 
        printfn "Mois : "
        let dateStartM = System.Console.ReadLine() |> int;
        printfn "Annee (YYYY)  "
        let dateStartY = System.Console.ReadLine() |> int;
        let DateStart = DateTime(dateStartY, dateStartM, dateStartJ);
        printfn "Votre debut de conge est le : %s" (DateStart.ToShortDateString())
        printfn "La matinee est elle comprise ? o/n"
        let matinStart = System.Console.ReadLine() |> char;
        let DateStartHaldDay = if (matinStart.Equals('o')) then AM else PM


        printfn "=============================== dateEnd : ==================================="
        printfn "Jour : "
        let dateEndJ = System.Console.ReadLine() |> int;
        printfn "Mois : "
        let dateEndM = System.Console.ReadLine() |> int;
        printfn "Annee (YYYY)  "
        let dateEndY = System.Console.ReadLine() |> int;
        let DateEnd = DateTime(dateEndY, dateEndM, dateEndJ);
        printfn "Votre fin de conge est le : %s" (DateEnd.ToShortDateString())
        printfn "L'apres-midi est il compris ? o/n"
        let apresMidiEnd = System.Console.ReadLine() |> char;
        let DateEndHaldDay = if (apresMidiEnd.Equals('o')) then PM else AM
        let command = Command.RequestTimeOff { UserId = idUser
                                               RequestId = new Guid();
                                               Start = { Date = DateStart; HalfDay = DateStartHaldDay }
                                               End = { Date = DateEnd; HalfDay = DateEndHaldDay } 
                                             }
        //Return RequestCreated
        //let result = Logic.handleCommand store command

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
            if user.UserId = int idUser then 
               testExist <- true;
        if testExist = false then printfn "Utilisateur non reconu, merci de recommencer";
        nbTry <- nbTry + 1;
    
    if testExist = false  then printfn "Trop de tentatives, retour au menu"
    //else
    //   store.GetStream.


let main() =
    let mutable i = 0;  
    while continueProg do
        
        printfn "========================Bienvenue dans le gestionnaire de conges. Merci de choisir une fonctionnalite.===============================";
        printfn "1 : Faire une demande de conges";
        printfn "2 : liste des demandes de conges";
        printfn "3 : consulter soldes d'un utilisateur";
        printfn "0 : Quitter";

        let choice =  System.Console.ReadLine();
        if choice = "1" then demandeConges();
        elif choice = "2" then listeConges();
        elif choice = "0" then continueProg <- false;
        else  printfn "Ce choix n'existe pas";
        
        printfn "Fin de l'action";


main();



