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
  | AddTakeAway of talkId: Guid * description: string
