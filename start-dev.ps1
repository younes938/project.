$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  throw 'لم يتم العثور على .NET SDK. ثبّت .NET SDK 10 ثم أعد تشغيل السكربت.'
}
if (-not (Get-Command node -ErrorAction SilentlyContinue) -or -not (Get-Command npm -ErrorAction SilentlyContinue)) {
  throw 'لم يتم العثور على Node.js/npm. ثبّت Node.js 22.12 أو أحدث ثم أعد تشغيل السكربت.'
}

$backend = Join-Path $root 'backend'
$frontend = Join-Path $root 'frontend'
$backendCommand = "Set-Location '$backend'; dotnet restore ETicket.slnx; if (`$LASTEXITCODE -eq 0) { dotnet run --project src/ETicket.Api/ETicket.Api.csproj --launch-profile http }"
$frontendCommand = "Set-Location '$frontend'; if (-not (Test-Path 'node_modules')) { npm ci }; if (`$LASTEXITCODE -eq 0) { npm start }"
Start-Process powershell -ArgumentList @('-NoExit', '-ExecutionPolicy', 'Bypass', '-Command', $backendCommand)
Start-Process powershell -ArgumentList @('-NoExit', '-ExecutionPolicy', 'Bypass', '-Command', $frontendCommand)
Write-Host 'تم فتح نافذتي الخادم والواجهة. تأكد أولًا من تنفيذ ملفي SQL، ثم افتح http://localhost:4200.' -ForegroundColor Green
