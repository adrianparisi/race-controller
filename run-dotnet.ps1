$ErrorActionPreference = "Stop"

$exe = Join-Path $PSScriptRoot "pc-dotnet\RaceController\bin\Debug\net10.0-windows\win-x64\RaceController.exe"

if (-not (Test-Path $exe)) {
    dotnet build (Join-Path $PSScriptRoot "pc-dotnet\RaceController\RaceController.csproj") -v minimal
}

& $exe `
    --port COM3 `
    --vjoy `
    --print `
    --invert-ch2 `
    --keyboard-throttle `
    --keyboard-throttle-key up `
    --keyboard-throttle-source brake `
    --suppress-brake-while-keyboard-throttle
