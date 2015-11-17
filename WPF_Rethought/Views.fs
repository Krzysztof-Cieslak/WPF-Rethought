namespace Views

open System
open System.Windows
open System.Windows.Controls
open FsXaml
open State
open Intent

open FSharp.Control.Reactive
open FSharp.Control.Reactive.Observable

type TextBoxView = XAML<"TextBoxView.xaml", true>

type TextBoxViewProvider() =
    inherit ViewProvider<TextBoxView, UserControl, string, TextBoxViewModel>(
        { 
            Cursor = CreateCursor ApplicationState.TextBox_
            EventStreams = fun (view : TextBoxView) ->
                [ Observable.fromIEvent view.txtTest.TextChanged |> Observable.map (fun _ -> view.txtTest.Text) ]
            Update = fun state event -> { state with Text = event}
        })

    member this.Text
        with get () = this.ModelStream |> Observable.map (fun n -> n.Text)

type TimerView = XAML<"TimerView.xaml", true> 

type TimerViewProvider () =
    inherit ViewProvider<TimerView, UserControl, DateTime, TimerViewModel>(
        {
            Cursor = CreateCursor ApplicationState.Timer_
            EventStreams = fun (view : TimerView) ->
                [ TimeSpan(0,0,1) |> Observable.interval |> Observable.map (fun _ -> DateTime.Now) ]
            Update = fun state event -> {state with Time = event }
        })

    member this.TimeString 
        with get () = this.ModelStream |> Observable.map (fun n -> n.Time.ToString ())
    
type CounterView = XAML<"CounterView.xaml", true> 

type CounterEvents =
    | Add 
    | Remove

type CounterViewProvider () = 
    inherit ViewProvider<CounterView, UserControl, CounterEvents, CounterViewModel>(
        {
            Cursor = CreateCursor ApplicationState.Counter_ 
            EventStreams = fun (view : CounterView) ->
                [ Observable.fromIEvent view.btnAdd.Click|> Observable.map (fun _ -> Add)
                  Observable.fromIEvent view.btnRemove.Click |> Observable.map (fun _ -> Remove) ]
            Update = fun state event -> 
                match event with
                | Add -> {state with Count = state.Count + 1 }
                | Remove -> {state with Count = state.Count - 1 }
        })
        
    member this.CounterString
        with get () = this.ModelStream |> Observable.map (fun n -> string n.Count )