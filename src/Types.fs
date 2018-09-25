module Types

open System

type TakeAway =
    { Id: Guid
      Description: string
      Votes: int }

type Talk =
    { Id: Guid
      Title: string
      TakeAways: TakeAway list
      // Not serialized
      NewTakeAway: string }

type Model = Talk list

type Msg =
  | VoteUp of talkId: Guid * takeId: Guid
  | UpdateNewTakeAway of talkId: Guid * string
  | AddTakeAway of talkId: Guid * description: string
  | GetTalks of Talk list
  | FetchError of ex: Exception

module Json =
  open Thoth.Json

  let takeAwayDecoder = Decode.Auto.generateDecoder<TakeAway>(isCamelCase = true)

  let talkDecoder: Decode.Decoder<Talk> = Decode.object (fun get ->
    { Id = get.Required.Field "id" Decode.guid
      Title = get.Required.Field "title" Decode.string
      TakeAways = get.Required.Field "takeAways" (Decode.list takeAwayDecoder)
      NewTakeAway = "" })

  let takeAwayEncode (x: TakeAway) = Encode.object [
      "id", Encode.guid x.Id
      "description", Encode.string x.Description
      "votes", Encode.int x.Votes
  ]

  let talkEncode (x: Talk) = Encode.object [
    "id", Encode.guid x.Id
    "title", Encode.string x.Title
    "takeAways", List.map takeAwayEncode x.TakeAways |> Encode.list
  ]
