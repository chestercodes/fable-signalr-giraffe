namespace Shared

type Counter = { Value : int }

module Constants =
    let hubHostedName = "gameHub"
    let hubServerUrlPart = sprintf "/%s" hubHostedName
    let hubClientUrl = sprintf "http://localhost:8085/%s" hubHostedName
