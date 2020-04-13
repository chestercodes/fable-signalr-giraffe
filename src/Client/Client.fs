module Client

open Elmish
open Elmish.React
open Fable.FontAwesome
open Fable.React
open Fable.React.Props
open Fulma
open Fable.Import.SignalR
open Fable.Core

//https://fable.io/docs/communicate/js-from-fable.html
let [<Global("signalR")>] sr:IExports = jsNative

// this is a bit grim, went to the global signalR object in console and tweaked until this matched that...
let connection = sr.HubConnectionBuilder.prototype.withUrl(Shared.Constants.hubClientUrl).build()

connection.start() |> ignore

type Model = { MessagesToServer: string list; MessagesFromServer: string list }

type Msg =
    | Start
    | SentMessageToServer of string
    | ReceivedMessageFromServer of string
    
let question = "Are we there yet?"

let sendQuestion q =
    let arr = ResizeArray([Some (q :> obj)])
    connection.invoke("SendToServer", arr)

let sendQuestionCmd q =
    Cmd.OfPromise.perform sendQuestion q (fun _ -> SentMessageToServer q)

let signalRSubscription initial =
    let sub dispatch =
        connection.on("SendToClient", fun (data) ->
            match data with
            | x when x.Count = 0 -> ()
            | x -> dispatch (ReceivedMessageFromServer (string x))
            | _ -> ()
        );
    Cmd.ofSub sub

let init () : Model * Cmd<Msg> =
    let initialModel = { MessagesToServer = []; MessagesFromServer = [] }
    initialModel, Cmd.none

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | Start ->
        currentModel, (sendQuestionCmd question)
    | SentMessageToServer msg ->
        let m = { currentModel with MessagesToServer = (currentModel.MessagesToServer @ [msg]) }
        m, Cmd.none
    | ReceivedMessageFromServer msg ->
        let m = { currentModel  with MessagesFromServer = (currentModel.MessagesFromServer @ [msg]) }
        m, (sendQuestionCmd question)

let safeComponents =
    let components =
        span [ ]
           [ a [ Href "https://github.com/SAFE-Stack/SAFE-template" ]
               [ str "SAFE  "
                 str Version.template ]
             str ", "
             a [ Href "https://github.com/giraffe-fsharp/Giraffe" ] [ str "Giraffe" ]
             str ", "
             a [ Href "http://fable.io" ] [ str "Fable" ]
             str ", "
             a [ Href "https://elmish.github.io" ] [ str "Elmish" ]
             str ", "
             a [ Href "https://fulma.github.io/Fulma" ] [ str "Fulma" ]
             str ", "
             a [ Href "https://bulmatemplates.github.io/bulma-templates/" ] [ str "Bulma\u00A0Templates" ]

           ]

    span [ ]
        [ str "Version "
          strong [ ] [ str Version.app ]
          str " powered by: "
          components ]

let navBrand =
    Navbar.Brand.div [ ]
        [ Navbar.Item.a
            [ Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
              Navbar.Item.IsActive true ]
            [ img [ Src "https://safe-stack.github.io/images/safe_top.png"
                    Alt "Logo" ] ] ]

let navMenu =
    Navbar.menu [ ]
        [ Navbar.End.div [ ]
            [ Navbar.Item.a [ ]
                [ str "Home" ]
              Navbar.Item.a [ ]
                [ str "Examples" ]
              Navbar.Item.a [ ]
                [ str "Documentation" ]
              Navbar.Item.div [ ]
                [ Button.a
                    [ Button.Color IsWhite
                      Button.IsOutlined
                      Button.Size IsSmall
                      Button.Props [ Href "https://github.com/SAFE-Stack/SAFE-template" ] ]
                    [ Icon.icon [ ]
                        [ Fa.i [Fa.Brand.Github; Fa.FixedWidth] [] ]
                      span [ ] [ str "View Source" ] ] ] ] ]


let containerBox (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ ]
        [ Field.div [ Field.IsGrouped ]
            [   Control.p [ ]
                    [ Button.a
                        [ Button.Color IsPrimary
                          Button.OnClick (fun _ -> dispatch Start) ]
                        [ str "Start asking" ] ] ] ]

let displayMessages (model : Model) =
    div [] [
        Message.message [ ]
            [ Message.body [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ str (String.concat System.Environment.NewLine model.MessagesToServer) ] ]
        Message.message [ ]
            [ Message.body [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ str (String.concat System.Environment.NewLine model.MessagesFromServer) ] ]
    ]
                    
let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [ Hero.Color IsPrimary; Hero.IsFullHeight ]
        [ Hero.head [ ]
            [ Navbar.navbar [ ]
                [ Container.container [ ]
                    [ navBrand
                      navMenu ] ] ]

          Hero.body [ ]
            [ Container.container [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ Column.column
                    [ Column.Width (Screen.All, Column.Is6)
                      Column.Offset (Screen.All, Column.Is3) ]
                    [ Heading.p [ ]
                        [ str "SAFE Template" ]
                      Heading.p [ Heading.IsSubtitle ]
                        [ safeComponents ]
                      containerBox model dispatch
                      displayMessages model] ] ] ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
|> Program.withSubscription signalRSubscription
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
