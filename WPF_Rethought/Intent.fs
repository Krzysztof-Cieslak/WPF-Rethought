module Intent

open System
open System.Windows
open System.Windows.Controls
open FsXaml
open State

open FSharp.Control.Reactive
open FSharp.Control.Reactive.Observable

type ViewIntent<'T, 'E, 'S when 'T :> XamlContainer> = {
    Cursor : State.Cursor<'S>
    EventStreams : 'T -> IObservable<'E> list
    Update : 'S -> 'E -> 'S
}

type ViewProvider<'T, 'U, 'E, 'S when 'U :> FrameworkElement and 'T :> XamlContainer> (intent : ViewIntent<'T,'E,'S>) =
    inherit ViewControllerBase<'T, 'U>()

    let (getState, update, stream) = intent.Cursor
    let disposables = ResizeArray ()

    member this.ModelStream = stream

    override this.OnInitialized view =
        view.DataContext <- this
        intent.EventStreams view
        |> Observable.mergeSeq
        |> Observable.subscribe (fun e -> 
            let state = getState ()
            let state' = intent.Update state e
            update state' )
        |> disposables.Add

    override this.OnUnloaded view =
        disposables |> Seq.iter (fun d -> d.Dispose () )
