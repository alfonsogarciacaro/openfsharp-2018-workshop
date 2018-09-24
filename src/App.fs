module App.View

open System
open Fable.Core.JsInterop
open Fable.Helpers.React
open Elmish
open Fulma
open Types

let registerServiceWorker(): unit = importMember "./js/util.js"

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
    [sample; sample], Cmd.none

let update msg model =
    match msg with
    | VoteUp(talkId, takeId) ->
        printfn "TODO: VoteUp"
        model, Cmd.none
    | AddTakeAway(talkId, takeId) ->
        printfn "TODO: AddTakeAway"
        model, Cmd.none

let view model dispatch =
    div [] (List.map (Card.view dispatch) model)

open Elmish.React

registerServiceWorker()

Program.mkProgram init update view
|> Program.withReact "elmish-app"
|> Program.run
