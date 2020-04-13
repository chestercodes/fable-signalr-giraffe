dotnet new SAFE --deploy azure --server giraffe --layout fulma-landing --force

dotnet fake build --target run

yarn add @microsoft/signalr



$d = $PSScriptRoot
[IO.Directory]::CreateDirectory("$d/temp")

$dtsFiles = gci "$d/node_modules/@microsoft/signalr/dist/esm" -filter *.d.ts | where {$_.Name -ne "index.d.ts"} | select -expandproperty FullName
$dtsFiles

$combinedName = "$d/temp/Signalr.d.ts"
[IO.File]::WriteAllText($combinedName, "")

foreach($n in $dtsFiles){
    $content = [IO.File]::ReadAllLines($n) | where { -not ($_ -match "import .*" )  }
    $content =  $content |% { $_.Replace("export default", "export").ToString() }
    $content = $content | out-string
    [IO.File]::AppendAllText($combinedName, $content)
}

npm install -g ts2fable
ts2fable $combinedName "$d/temp/SignalR.fs"
