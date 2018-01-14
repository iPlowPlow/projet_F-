namespace TimeOff
module TimeOff = 

    open System
    open Expecto
    open System.Collections.Generic
    open TimeOff
    open TimeOff.Repositories
    open TimeOff.Restful
    open EventStorage
    open Logic


    open Suave
    open Suave.Web

    [<EntryPoint>]
    let main argv =

        let userRepository = {
            CreateTimeOff = Db.createTimeOffRequest
            CancelTimeOffByEmployee = Db.cancelTimeOffByEmployee
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


//let mutable continueProg = true;
//let store = InMemoryStore.Create<UserId, RequestEvent>()

//let usersList = new List<Person>()
//usersList.Add(new Person (userId= 1, name = "Azedine" ))
//usersList.Add(new Person (userId= 2, name = "Loic" ))
//usersList.Add(new Person (userId= 3, name = "Andre" ))
//usersList.Add(new Person (userId= 3, name = "Bob" ))

//let manager = User.Manager;



//let displayConges id = 
//    let stream = store.GetStream id
//    let listEventsUser = stream.ReadAll();
//    if Seq.length listEventsUser = 0 then printfn "Aucune demande de congés pour cette utilisateur"
//    else 
       
//        for event in listEventsUser do 
//            let tmpSeq = seq { yield event }
//            let statEvent = getRequestState tmpSeq
//            let dispStat = statEvent.GetType()
//            printfn "===============================Demande numéros : %A ===============================" event.Request.RequestId
//            printfn "Nom : %s" (string (Logic.getUserName(id, usersList)))
//            printfn "Date debut :  %A %A" event.Request.Start.Date event.Request.Start.HalfDay
//            printfn "Date fin :  %A %A" event.Request.End.Date event.Request.End.HalfDay
           
//            printfn "Etat de la demande : %s"  dispStat.Name
//            printfn ""




//let demandeConges() = 
//    printfn "==========================Faire une demande de conges====================="
//    printfn "Merci de renseigner les champs suivants : \" idUser, dateStart, dateEnd\" "
    
//    let mutable testExist = false;
//    let mutable nbTry = 0;
//    printfn "idUser : ";
//    let mutable idUser = 0;
//    while testExist = false && nbTry <3  do
//        idUser <- System.Console.ReadLine() |> int;
//        for user in usersList do
//            if user.UserId = int idUser then 
//                testExist <- true;
//        if testExist = false then printfn "Utilisateur non reconu, merci de recommencer";
//        nbTry <- nbTry + 1;
    
//    if testExist = false  then printfn "Trop de tentatives, retour au menu"
//    else 
//        printfn "Utilisateur selectionne: %s" (string (Logic.getUserName(idUser, usersList)))
//        printfn "=============================== dateStart : ==================================="
//        printfn "Jour : "
//        let dateStartJ = System.Console.ReadLine() |> int; 
//        printfn "Mois : "
//        let dateStartM = System.Console.ReadLine() |> int;
//        printfn "Annee (YYYY)  "
//        let dateStartY = System.Console.ReadLine() |> int;
//        let DateStart = DateTime(dateStartY, dateStartM, dateStartJ);
//        printfn "Votre debut de conge est le : %s" (DateStart.ToShortDateString())
//        printfn "La matinee est elle comprise ? o/n"
//        let matinStart = System.Console.ReadLine() |> char;
//        let DateStartHaldDay = if (matinStart.Equals('o')) then AM else PM


//        printfn "=============================== dateEnd : ==================================="
//        printfn "Jour : "
//        let dateEndJ = System.Console.ReadLine() |> int;
//        printfn "Mois : "
//        let dateEndM = System.Console.ReadLine() |> int;
//        printfn "Annee (YYYY)  "
//        let dateEndY = System.Console.ReadLine() |> int;
//        let DateEnd = DateTime(dateEndY, dateEndM, dateEndJ);
//        printfn "Votre fin de conge est le : %s" (DateEnd.ToShortDateString())
//        printfn "L'apres-midi est il compris ? o/n"
//        let apresMidiEnd = System.Console.ReadLine() |> char;
//        let DateEndHaldDay = if (apresMidiEnd.Equals('o')) then PM else AM
//        let command = Command.RequestTimeOff { UserId = idUser
//                                               RequestId = Guid.NewGuid();
//                                               Start = { Date = DateStart; HalfDay = DateStartHaldDay }
//                                               End = { Date = DateEnd; HalfDay = DateEndHaldDay } 
//                                             }


//        //Return RequestCreated
//        let result = Logic.handleCommand store command
//        match result with
//            | Ok events ->
//                for event in events do
//                  let stream = store.GetStream event.Request.UserId
//                  stream.Append [event]

//            | Error e -> printfn "Error: %s" e
            
//        printfn "Fin de la demande de conges"





//let listeConges() = 
//    printfn "liste des demandes de conges"
//    let mutable testExist = false;
//    let mutable nbTry = 0;
//    printfn "idUser : (0 for all)";
//    let mutable idUser = 0;
//    while testExist = false && nbTry <3  do
//        idUser <- System.Console.ReadLine() |> int;
//        if idUser = 0 then 
//            testExist <- true;
//        for user in usersList do
//            if user.UserId = int idUser then 
//               testExist <- true;
//        if testExist = false then printfn "Utilisateur non reconu, merci de recommencer";
//        nbTry <- nbTry + 1;
    
//    if testExist = false  then printfn "Trop de tentatives, retour au menu"
//    elif idUser = 0 then
//         printfn "=============================== All ===============================" 
//         for user in usersList do
//            displayConges user.UserId
//    else
//        displayConges idUser
        







//let updateDemandeConges() =
//    printfn "========================== Valider/refuser une demande de conges====================="
//    let mutable testExist = false;
//    let mutable nbTry = 0;
//    printfn "idUser :";
//    let mutable idUser = 0;
//    while testExist = false && nbTry <3  do
//        idUser <- System.Console.ReadLine() |> int;
//        for user in usersList do
//            if user.UserId = int idUser then 
//               testExist <- true;
//        if testExist = false then printfn "Utilisateur non reconu, merci de recommencer";
//        nbTry <- nbTry + 1;
    
//    if testExist = false  then printfn "Trop de tentatives, retour au menu"
//    else 
//        let stream = store.GetStream idUser
//        let listEventsUser = stream.ReadAll();
//        if Seq.length listEventsUser = 0 then printfn "Aucune demande de congés pour cette utilisateur"
//        else 
//            printfn "Liste des congés a traiter :"
//            for event in listEventsUser do 
//                let tmpSeq = seq { yield event }
//                let statEvent = getRequestState tmpSeq
//                let dispStat = statEvent.GetType()
//                if(dispStat.Name = "PendingValidation") then
//                    printfn "%A" event.Request.RequestId
//            printfn "GUID :"
//            let choiceGUID =  System.Console.ReadLine();    
//            printfn "Valider ou refuser (v/r) :";
//            let action =  System.Console.ReadLine();  
//            let myEvent = new List<RequestEvent>()
//            for event in listEventsUser do 
//                if event.Request.RequestId.ToString() = choiceGUID then
//                    myEvent.Add(event)
//                    printf ""
//            if(action = "v") then                     
//                let command = Command.ValidateRequest (idUser, myEvent.[0].Request.RequestId)
//                let result = Logic.handleCommand store command
//                match result with
//                    | Ok events ->
//                        for event in events do
//                            let stream = store.GetStream event.Request.UserId
//                            stream.Append [event]
//                    | Error e -> printfn "Error: %s" e
                    
//                printfn "Demande Validée"
                        




//let main() =
//    let mutable i = 0;  
//    while continueProg do
        
//        printfn "========================Bienvenue dans le gestionnaire de conges. Merci de choisir une fonctionnalite.===============================";
//        printfn "1 : Faire une demande de conges";
//        printfn "2 : Liste des demandes de conges";
//        printfn "3 : Valider/refuser une demande de conges (manageur)";
//        printfn "4 : Consulter soldes d'un utilisateur";
//        printfn "0 : Quitter";

//        let choice =  System.Console.ReadLine();
//        if choice = "1" then demandeConges();
//        elif choice = "2" then listeConges();
//        elif choice = "3" then updateDemandeConges();
//        elif choice = "0" then continueProg <- false;
//        else  printfn "Ce choix n'existe pas";
        
//        printfn "Fin de l'action";


//main();



