open System
open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection

open FSharp.Control.Tasks.V2
open Giraffe
open Shared

open Microsoft.WindowsAzure.Storage

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = tryGetEnv "public_path" |> Option.defaultValue "../Client/public" |> Path.GetFullPath
let storageAccount = tryGetEnv "STORAGE_CONNECTIONSTRING" |> Option.defaultValue "UseDevelopmentStorage=true" |> CloudStorageAccount.Parse
let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let webApp =
    route "/api/init" >=>
        fun next ctx ->
            task {
                let counter = { Value = 42 }
                return! json counter next ctx
            }

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles()
        .UseCors()
        .UseStaticFiles()
        .UseRouting()
        .UseEndpoints(fun ep -> ep.MapHub<Hub.GameHub>(Shared.Constants.hubServerUrlPart) |> ignore)
        .UseGiraffe webApp
    ()
       
let configureServices (services : IServiceCollection) =
    services.AddCors(fun opt ->
        opt.AddDefaultPolicy(fun b ->
            b.WithOrigins("http://localhost:8080").AllowAnyHeader().AllowAnyMethod().AllowCredentials() |> ignore
            ()
        )
    ) |> ignore
    services.AddSignalR () |> ignore
    services.AddGiraffe() |> ignore

WebHost
    .CreateDefaultBuilder()
    .UseWebRoot(publicPath)
    .UseContentRoot(publicPath)
    .Configure(Action<IApplicationBuilder> configureApp)
    .ConfigureServices(configureServices)
    .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
    .Build()
    .Run()
