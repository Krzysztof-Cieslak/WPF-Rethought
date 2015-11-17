module State

open System
open Aether
open Aether.Operators
open FSharp.Control.Reactive
open FSharp.Control.Reactive.Observable

type TextBoxViewModel = {
    Text : string
}

type CounterViewModel = {
    Count : int
}

type TimerViewModel = {
    Time : DateTime
}

type ApplicationState = {
    TextBox  : TextBoxViewModel
    Counter  : CounterViewModel
    Timer    : TimerViewModel
}
with 
    static member Zero = { 
        TextBox = { Text  = "" } 
        Counter = { Count = 0 }
        Timer   = { Time  = DateTime.Now }
    }
    static member TextBox_ = 
        (fun x -> x.TextBox), (fun t x -> { x with TextBox = t })
    static member Counter_ = 
        (fun x -> x.Counter), (fun t x -> { x with Counter = t })
    static member Timer_ = 
        (fun x -> x.Timer), (fun t x -> { x with Timer = t })
 
let mutable private states = [ApplicationState.Zero]

let private stateUpdated = Event<ApplicationState> ()
let private stateUpdate = stateUpdated.Publish

type Cursor<'a> = (unit -> 'a) * ('a -> unit) * IObservable<'a> 

let CreateCursor (lens : Lens<ApplicationState, 'a>) : Cursor<'a> = 
    let getter () = 
        states |> List.head |> Lens.get lens
    
    let setter value =
        let state = states |> List.head |> Lens.set lens value
        states <- state :: states
        stateUpdated.Trigger state
    
    let stream = 
        stateUpdate
        |> Observable.map (Lens.get lens)
        |> Observable.distinctUntilChanged
    
    (getter, setter, stream)