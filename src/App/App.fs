module App.View

open System
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.PowerPack
open Thoth.Json
open Elmish
open Fulma
open Global
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
    let talks = [
        { Id = Guid.NewGuid()
          Title = "Fable 2 is coming!"
          TakeAways = [
            { Id = Guid.NewGuid()
              Description = "Many improvements!"
              Votes = 5 }
          ]
          NewTakeAway = ""
        }
    ]
    let model = { Talks = talks; HavingFun = false; FunsnakeCom = None }
    // let cmd = Cmd.ofPromise (Rest.get GET_TALKS (Decode.list Json.talkDecoder)) ()
    //                 GetTalksSuccess FetchError
    model, Cmd.none

let update msg model =
    match msg with
    | FetchError err ->
        Log.Error("[FETCH] " + err.Message)
        model, Cmd.none
    | GetTalksSuccess talks ->
        { model with Talks = talks }, Cmd.none
    | VoteUp(talkId, take) ->
        model, Cmd.none
        // let url = sprintf "%s/%s" POST_VOTE (string talkId)
        // let take = { take with Votes = take.Votes + 1 }
        // model, Cmd.ofPromise (Rest.post url Json.takeAwayEncode Json.takeAwayDecoder) take
        //     (fun take -> VoteUpSuccess(talkId, take)) FetchError
    | VoteUpSuccess(talkId, take) ->
        let talks = model.Talks |> List.replaceById talkId (fun x ->
            { x with TakeAways = x.TakeAways |> List.replaceById take.Id (fun _ -> take) })
        { model with Talks = talks }, Cmd.none
    | UpdateNewTakeAway(talkId, description) ->
        let talks = model.Talks |> List.replaceById talkId (fun x ->
            { x with NewTakeAway = description })
        { model with Talks = talks }, Cmd.none

let view (model: Model) dispatch =
    div [] [
        yield! List.map (Card.view dispatch) model.Talks
        yield br []
    ]

open Elmish.React

Program.mkProgram init update view
|> Program.withReact "elmish-app"
|> Program.run
