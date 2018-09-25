module Prelude

open System
open Fable.Core

type Log =
    [<Emit("console.log($0, ...$1)")>]
    static member Info(msg: string, [<ParamArray>] args: obj[]) = jsNative

    [<Emit("console.error($0, ...$1)")>]
    static member Error(msg: string, [<ParamArray>] args: obj[]) = jsNative

[<RequireQualifiedAccess>]
module List =
    let inline replaceById< ^Id, ^T when ^T : (member Id: ^Id) and ^Id : equality>
                    (id: ^Id) (f: ^T -> ^T) (xs: ^T list) =
        xs |> List.map (fun x ->
            let id2 = (^T : (member Id: ^Id) x)
            if id = id2 then f x else x)
