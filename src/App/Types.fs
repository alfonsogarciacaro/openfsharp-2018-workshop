module Types

open System
open Fable.Import

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

type Model =
    { Talks: Talk list
      HavingFun: bool
      FunsnakeCom: React.ComponentClass<obj> option }

type Msg =
  | VoteUp of talkId: Guid * take: TakeAway
  | VoteUpSuccess of talkId: Guid * TakeAway
  | UpdateNewTakeAway of talkId: Guid * string
  | GetTalksSuccess of Talk list
  | FetchError of ex: Exception
