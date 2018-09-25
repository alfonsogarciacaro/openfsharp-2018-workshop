[<RequireQualifiedAccess>]
module Card

open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fulma.FontAwesome
open Types

let buttonCell disabled badge icon onClick =
    th [] [
        Button.button [
            match badge with
            | Some value ->
                yield Button.CustomClass "badge is-badge-success"
                yield Button.Props [Data("badge", value)]
            | None -> ()
            yield Button.Disabled disabled
            yield Button.OnClick (fun _ -> onClick())
        ] [
            Icon.faIcon [] [
                Fa.icon icon
                Fa.faLg
            ]
        ]
    ]


let newTakeAway dispatch (talk: Talk) =
    tr [] [
        th [] [
            Control.div [] [
                Input.text [
                    Input.Placeholder "What did you learn?"
                    Input.Value talk.NewTakeAway
                    Input.OnChange (fun ev ->
                        UpdateNewTakeAway(talk.Id, ev.Value) |> dispatch)
                ]
            ]
        ]
        buttonCell (String.IsNullOrEmpty talk.NewTakeAway) None Fa.I.Plus (fun _ ->
            AddTakeAway(talk.Id, talk.NewTakeAway) |> dispatch)
    ]

let takeAway dispatch (talk: Talk) (take: TakeAway) =
    tr [] [
        th [] [str take.Description]
        buttonCell false (Some take.Votes) Fa.I.ThumbsUp (fun _ ->
            VoteUp(talk.Id, take) |> dispatch)
    ]

let view dispatch (talk: Talk) =
    Card.card [CustomClass "talk-card"] [
        Card.header [] [
            Card.Header.title [] [ str talk.Title ]
            // Card.Header.icon [] [ i [ Class "fa fa-angle-down" ] [] ]
        ]
        Card.content [] [
            Table.table [
                Table.IsHoverable
                Table.IsBordered
                Table.IsFullWidth
                Table.IsStriped
            ] [
                // thead [] [ tr [] [ th [] [] ] ]
                tbody [] [
                    yield! List.map (takeAway dispatch talk) talk.TakeAways
                    yield newTakeAway dispatch talk
                ]
            ]
        ]
        // Card.footer [] [
        //     Card.Footer.item [] [ str "Save" ]
        // ]
    ]
