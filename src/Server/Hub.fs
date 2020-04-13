module Hub

open Microsoft.AspNetCore.SignalR
open System.Threading.Tasks

type IClientApi = 
  abstract member SendToClient :string -> Task

type GameHub () =
    inherit Hub<IClientApi> ()
    member this.SendToServer (message: string) =
        let connectionId = this.Context.ConnectionId
        System.Console.WriteLine(sprintf "%s -> %s" connectionId message)

        System.Threading.Thread.Sleep(1000)

        this.Clients.Clients(connectionId).SendToClient("Not yet...") |> ignore

        Task.CompletedTask
