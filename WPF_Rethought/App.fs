module main

open System
open FsXaml

type App = XAML<"App.xaml">

[<STAThread>]
[<EntryPoint>]
let main argv =
    try
        App().Root.Run()
    with
    | ex ->
        let t = sprintf "%O" ex
        -1