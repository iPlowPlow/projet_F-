module TimeOff.TestsRunner

open System
open Expecto

let newEmployee = User.Employee 1;

let name = System.Console.ReadLine()

printfn "name"

[<EntryPoint>]
let main args =
  runTestsWithArgs { defaultConfig with ``parallel`` = false } args Tests.tests