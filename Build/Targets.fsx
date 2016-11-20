#I "../packages/FAKE.4.45.2/tools"
#I "../packages/FSharp.Data.2.3.2/lib/net40"

#r "FakeLib.dll"
#r "FSharp.Data.dll"

open System
open Fake 
open FSharp.Data

type GlobalJson = JsonProvider<"../global.json">
let globalJson = GlobalJson.GetSample()
let dotNetCoreProjects = globalJson.Projects

// Default target
Target "Build" <| fun _ -> traceHeader "STARTING BUILD"

Target "Clean" (fun _ ->
    !! "**/obj" ++ "**/bin" ++ "artifacts" |> CleanDirs
)

Target "Restore" (fun _ ->
    dotNetCoreProjects
    |> Array.iter(fun p -> 
        DotNetCli.Restore(fun c -> { c with Project = p; AdditionalArgs = ["--verbosity Warning"] })
    )
)

Target "BuildApp" (fun _ ->
    dotNetCoreProjects
    |> Array.iter(fun p -> 
        DotNetCli.Build(fun c -> { c with Project = p; Configuration = "Release" })
    )
)

Target "Test"  <| fun _ -> DotNetCli.Test(fun c -> { c with Project = "Facts.DiffPlex"; Configuration = "Release" })

// Dependencies
"Clean" 
  ==> "Restore"
  ==> "BuildApp"
  =?> ("Test", (not ((getBuildParam "skiptests") = "1")))
  ==> "Build"

// start build
RunTargetOrDefault "Build"
