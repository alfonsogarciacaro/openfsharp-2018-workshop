module Global

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import

[<AutoOpen>]
module Literals =
    let [<Literal>] SERVICE_WORKER_PATH = "service-worker.js"
    let [<Literal>] GET_TALKS = "/api/talks"
    let [<Literal>] POST_TAKEAWAY ="/api/takeaways"
    let [<Literal>] POST_VOTE ="/api/vote"


type Log =
    static member Info(msg: string) = Browser.console.log(msg)
    [<Emit("console.log($0, ...$1)")>]
    static member Info(msg: string, [<ParamArray>] args: obj[]) = jsNative
    static member Error(msg: string) = Browser.console.error(msg)
    [<Emit("console.error($0, ...$1)")>]
    static member Error(msg: string, [<ParamArray>] args: obj[]) = jsNative

[<RequireQualifiedAccess>]
module List =
    let inline replaceById< ^Id, ^T when ^T : (member Id: ^Id) and ^Id : equality>
                    (id: ^Id) (f: ^T -> ^T) (xs: ^T list) =
        xs |> List.map (fun x ->
            let id2 = (^T : (member Id: ^Id) x)
            if id = id2 then f x else x)


[<Emit("'serviceWorker' in navigator")>]
let isServiceWorkerSupported(): bool = jsNative

let registerServiceWorker (path: string): unit =
    if isServiceWorkerSupported() then
        Browser.navigator?serviceWorker?register(path)

let unRegisterServiceWorkers (): unit =
    if isServiceWorkerSupported() then
        Browser.navigator?serviceWorker?getRegistrations()?``then``(fun (registrations: obj[]) ->
            for registration in registrations do
                registration?unregister()
        )
