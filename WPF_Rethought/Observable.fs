module Observable

open System.Reactive.Linq

let fromIEvent (event : IEvent<_,_>) = 
    event.AsObservable ()

