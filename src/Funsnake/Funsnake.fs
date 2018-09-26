module Funsnake.Main

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser

type DisposableToken() =
    member val Disposed = false with get, set

type Direction = Left | Right | Up | Down | None

// Initial values
let sizeLink = 10.
let canvasSize = (300., 300.)
let snake = [(sizeLink * 5.,sizeLink * 3.0,sizeLink,sizeLink);(sizeLink * 4.,sizeLink * 3.0,sizeLink,sizeLink);(sizeLink * 3.,sizeLink * 3.0,sizeLink,sizeLink);]
let wallTop   = ([0.0..sizeLink..(fst canvasSize)] |> List.map (fun x-> (x,0.,sizeLink,sizeLink)))
let wallDown  = ([0.0..sizeLink..(fst canvasSize)] |> List.map (fun x-> (x,(snd canvasSize),sizeLink,sizeLink)))
let wallLeft  = ([0.0..sizeLink..(snd canvasSize)] |> List.map (fun x-> (0.,x,sizeLink,sizeLink)))
let wallRight = ([0.0..sizeLink..(snd canvasSize)] |> List.map (fun x-> ((fst canvasSize),x,sizeLink,sizeLink)))
let wall = wallLeft @ wallTop @ wallRight @ wallDown
let mutable direction = None   // Control snake direction
let mutable moveDone = true     // Avoid direction changes until move has done
let mutable touch = (-1.,-1.)


// Helper functions

// Check if element exist in a generic list
let Contains (e:'T) (el:'T list) = el |> List.exists (fun x-> x = e)

// Get random number with max value
let getRandomAbsolute max absolute = (floor( (JS.Math.random() * max) / absolute)) * absolute

// Create a gradient color for then link rectange (simulates a circular link)
let defaultGradientLink (ctx:CanvasRenderingContext2D) (link :(float*float*float*float)) colorStart colorEnd =
        let x,y,h,w = link
        let gradient =ctx.createRadialGradient(x + (w / 2.), y + (h / 2.), 1., x+ (w /2.), y + (h / 2.), sizeLink - 4.)
        gradient.addColorStop(0.,colorStart)
        gradient.addColorStop(1.,colorEnd)
        U3.Case2 gradient

// Get the color for the link (orange = alive / red = collision)
let getColorLink (ctx:CanvasRenderingContext2D) (link :(float*float*float*float)) collision aliveColor collisionColor =
    if collision
    then defaultGradientLink ctx link collisionColor "white"
    else defaultGradientLink ctx link aliveColor "white"


/// skips last element of an array
let skipLast xs =
    let rec auxSkipLast current rest =
        match rest with
        | [] -> []
        | head :: [lastElement] -> List.append current [head]
        | head :: tail -> auxSkipLast (List.append current [head]) tail
    auxSkipLast [] xs

// Touch / Mouse listeners
[<Emit("$0 in $1")>]
let checkIn (listener: string) (o: obj) : bool = jsNative

let setTouchListener (canvas: HTMLCanvasElement) =
  if (checkIn "ontouchstart" canvas) then
      // Capture touchstart for mobile / touch devices
      canvas.addEventListener_touchstart(fun touchEv ->
          let ev = touchEv.changedTouches.Item 0
          touch <- (ev.pageX - canvas.offsetLeft, ev.pageY - canvas.offsetTop))
  else
      // Capture mousedown for browsers
      canvas.addEventListener_mousedown(fun ev ->
          touch <- (ev.pageX - canvas.offsetLeft, ev.pageY - canvas.offsetTop))
  |> ignore


// Move the snake to next position, if snake eat some food increase snake size in one link
let move xMove yMove snake food =
    match snake with
    | (x,y,h,w)::_ ->
        let newHead = (x + xMove, y + yMove , h, w)
        if (newHead = food)
        then newHead :: snake
        else newHead :: (snake |> skipLast)
    | _ -> snake

// Move direction shortcuts
let moveRight snake food = move sizeLink 0. snake food
let moveLeft  snake food = move -sizeLink 0. snake food
let moveUp    snake food = move 0. -sizeLink snake   food
let moveDown  snake food = move 0. sizeLink snake  food

// Generate a new random food place (avoid wall & snake position)
let rec newFood snake () =
    let randomFood = ( (getRandomAbsolute ((fst canvasSize) - sizeLink * 2.) sizeLink) + sizeLink, (getRandomAbsolute ((snd canvasSize) - sizeLink * 2.) sizeLink) + sizeLink, sizeLink, sizeLink)
    if snake |> Contains randomFood
    then newFood snake ()
    else randomFood

// Detect snake collision (against wall or itself)
let hasCollision (snake:(float*float*float*float) List) = wall |> Contains snake.Head || snake.Tail |> Contains snake.Head

// Draw snake and food in the canvas
let draw (canvas: HTMLCanvasElement) (snake:(float*float*float*float) List) (food:float*float*float*float) (hasCollision: bool) =
    let ctx = canvas.getContext_2d()
    ctx.clearRect(sizeLink, sizeLink, fst canvasSize - (sizeLink), snd canvasSize - (sizeLink)) // Avoid reset the wall

    // Draw snake head
    ctx.fillStyle <- defaultGradientLink ctx snake.Head "rgb(184,7,7)" "white"
    ctx.fillRect(snake.Head)

    // Draw snake tail
    snake.Tail |> List.iter (fun x->
        match x with
        | x,y,w,h ->
            ctx.fillStyle <- getColorLink ctx (x, y, w, h) hasCollision "orange" "red"
            ctx.fillRect(x, y, w, h)
    ) |> ignore

    // Draw canvas
    ctx.fillStyle <- defaultGradientLink ctx food "rgb(50,165,12)" "white"
    ctx.fillRect(food) |> ignore

// Draw the walls
let drawWall (canvas: HTMLCanvasElement) (wall:(float*float*float*float) List) =
    let ctx = canvas.getContext_2d()

    wall |> List.iter (fun x->
        ctx.fillStyle <- U3.Case1 "black"
        match x with
        | x,y,w,h -> ctx.fillRect(x, y, w, h)
    ) |> ignore


let drawGameOver (canvas: HTMLCanvasElement) =
    let ctx = canvas.getContext_2d()
    ctx.fillStyle <- U3.Case1 "red"
    ctx.font <- "18px Segoe UI";
    let metrics = ctx.measureText("Game over!!!")
    ctx.fillText("Game Over!!!", (fst canvasSize / 2.) - (metrics.width / 2.), (snd canvasSize) / 2.) |> ignore

// ------------------------------------------------------------------
// Recursive update function that process the game
let rec update (token: DisposableToken) (canvas: HTMLCanvasElement) (snake:(float*float*float*float) list) food () =

    // Determine the movement based on the position of the snake's head and the touch position (if touch control)
    let sx,sy,_,_ = snake.Head
    direction <-
        match touch with
        | (-1.,-1.) -> direction
        | ( x , y ) when y < sy && (direction = None || direction = Left || direction = Right) -> Up
        | ( x , y ) when y > sy && (direction = None || direction = Left || direction = Right) -> Down
        | ( x , y ) when x < sx && (direction = None || direction = Up || direction = Down) -> Left
        | ( x , y ) when x > sx && (direction = None || direction = Up || direction = Down) -> Right
        | (_,_) -> direction

    // Reset touch position
    touch <- (-1.,-1.)

    // Snake position based on cursor direction input
    let snake =
        match direction with
        | Right -> moveRight snake food
        | Left  -> moveLeft  snake food
        | Up    -> moveUp    snake food
        | Down  -> moveDown  snake food
        | None  -> snake

    // If snake ate some food generate new random food
    let food =
        if (snake.Head = food)
        then newFood snake ()
        else food

    // Detect snake collision
    let collision = hasCollision snake

    // Draw snake & food in canvas (collision is use for paint snake in red in case of collision)
    draw canvas snake food collision

    // Snake movement completed
    moveDone <- true

    // If collision, game over, otherwise, continue updating the game
    if collision || token.Disposed then
        drawWall canvas wall
        drawGameOver canvas
    else
        window.setTimeout(update token canvas snake food, 100) |> ignore

// ------------------------------------------------------------------
// Main function
let init token canvas =

    // Capture arrows keys to move the snake
    window.addEventListener_keydown(fun e ->
        if moveDone then
            if e.keyCode = 65. && (direction = None || direction = Up || direction = Down) then direction <- Left
            if e.keyCode = 87. && (direction = None || direction = Right || direction = Left) then direction <- Up
            if e.keyCode = 68. && (direction = None || direction = Up || direction = Down) then direction <- Right
            if e.keyCode = 83. && (direction = None || direction = Right || direction = Left) then direction <- Down
            moveDone <- false
        :> obj)

    // Capture touch events
    setTouchListener canvas

    // Draw the walls only once
    drawWall canvas wall

    // Start the game with basic snake and ramdom food
    update token canvas snake (newFood snake ()) () |> ignore

// React component
open System
open FSharp.Core
open Fable.Helpers.React
open Fable.Helpers.React.Props

type ReactRef<'T> =
    abstract current: 'T

let createRef<'T>(): ReactRef<'T> = importMember "react"

type Component() =
    inherit React.Component<obj, obj>()
    let myRef = createRef<HTMLCanvasElement>()
    let mutable myDisp: IDisposable option = None

    override __.componentDidMount() =
        myDisp <-
            let token = DisposableToken()
            init token myRef.current
            Some { new IDisposable with
                    member __.Dispose() =
                        token.Disposed <- true }

    override __.componentWillUnmount() =
        match myDisp with
        | None -> ()
        | Some d -> d.Dispose()

    override __.render() =
        let width, height = canvasSize
        canvas [
            HTMLAttr.Width (sprintf "%.0fpx" (width + sizeLink * 2.))
            HTMLAttr.Height (sprintf "%.0fpx" (height + sizeLink * 2.))
            Ref !!myRef
        ] []