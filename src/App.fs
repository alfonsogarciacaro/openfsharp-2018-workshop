module App.View

open System
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.PowerPack
open Thoth.Json
open Elmish
open Fulma
open Prelude
open Types

[<RequireQualifiedAccess>]
module Rest =
    open Fable.PowerPack.Fetch.Fetch_types

    let [<Literal>] GET_TALKS = "/api/talks"

    let get url (decoder: Decode.Decoder<'T>) () =
        Fetch.fetch url []
        |> Promise.bind (fun res -> res.json())
        |> Promise.map (fun json ->
            Decode.unwrap "$" decoder json)

    let post url (encoder: 'T1->Encode.Value) (decoder: Decode.Decoder<'T2>) data =
        Fetch.fetch url [
            Method HttpMethod.POST
            Fetch.requestHeaders [ContentType "application/json"]
            Body !^(encoder data |> Encode.toString 0)
        ]
        |> Promise.bind (fun res -> res.json())
        |> Promise.map (fun json ->
            Decode.unwrap "$" decoder json)

let init () =
    [], Cmd.ofPromise (Rest.get Rest.GET_TALKS (Decode.list Json.talkDecoder)) () GetTalks FetchError

let update msg model =
    match msg with
    | FetchError err ->
        Log.Error("[FETCH]", err.Message)
        model, Cmd.none
    | GetTalks talks ->
        talks, Cmd.none
    | VoteUp(talkId, takeId) ->
        printfn "TODO: VoteUp"
        model, Cmd.none
    | AddTakeAway(talkId, description) ->
        printfn "TODO: AddTakeAway %s" description
        model, Cmd.none
    | UpdateNewTakeAway(talkId, description) ->
        model |> List.replaceById talkId (fun x -> { x with NewTakeAway = description }), Cmd.none

let view model dispatch =
    div [] (List.map (Card.view dispatch) model)

open Elmish.React

Program.mkProgram init update view
|> Program.withReact "elmish-app"
|> Program.run
