module App.View

open System
open Fable.Core.JsInterop
open Fable.Helpers.React
open Elmish
open Fulma
open Prelude
open Types

let init () =
    let sample =
        { Id = Guid.NewGuid()
          Title = "Fable 2 is coming!"
          TakeAways = [
            { Id = Guid.NewGuid(); Description = "This is good"; Votes = 5 }
            { Id = Guid.NewGuid(); Description = "I'll wait for the next version"; Votes = 54 }
          ]
          NewTakeAway = ""
        }
    [sample; { sample with Id = Guid.NewGuid() }], Cmd.none

let update msg model =
    match msg with
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
