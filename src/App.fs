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
    [], Cmd.ofPromise (Rest.get GET_TALKS (Decode.list Json.talkDecoder)) ()
            GetTalksSuccess FetchError

let update msg model =
    match msg with
    | FetchError err ->
        Log.Error("[FETCH] " + err.Message)
        model, Cmd.none
    | GetTalksSuccess talks ->
        talks, Cmd.none
    | VoteUp(talkId, take) ->
        let url = sprintf "%s/%s" POST_VOTE (string talkId)
        let take = { take with Votes = take.Votes + 1 }
        model, Cmd.ofPromise (Rest.post url Json.takeAwayEncode Json.takeAwayDecoder) take
            (fun take -> VoteUpSuccess(talkId, take)) FetchError
    | VoteUpSuccess(talkId, take) ->
        model |> List.replaceById talkId (fun x ->
            { x with TakeAways = x.TakeAways |> List.replaceById take.Id (fun _ -> take) }), Cmd.none
    | AddTakeAway(talkId, description) ->
        let take =
            { Id = Guid.NewGuid()
              Description = description
              Votes = 1 }
        let url = sprintf "%s/%s" POST_TAKEAWAY (string talkId)
        model, Cmd.ofPromise (Rest.post url Json.takeAwayEncode Json.takeAwayDecoder) take
            (fun take -> AddTakeAwaySuccess(talkId, take)) FetchError
    | AddTakeAwaySuccess(talkId, take) ->
        model |> List.replaceById talkId (fun x ->
            { x with TakeAways = x.TakeAways @ [take] }), Cmd.none
    | UpdateNewTakeAway(talkId, description) ->
        model |> List.replaceById talkId (fun x -> { x with NewTakeAway = description }), Cmd.none

let view model dispatch =
    div [] (List.map (Card.view dispatch) model)

open Elmish.React

Program.mkProgram init update view
|> Program.withReact "elmish-app"
|> Program.run

#if DEBUG
unRegisterServiceWorkers()
#else
registerServiceWorker(SERVICE_WORKER_PATH)
#endif