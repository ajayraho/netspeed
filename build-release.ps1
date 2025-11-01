# Build script for creating release executable
# Run this script in PowerShell to create a distributable exe

Write-Host "[BUILD] Building Network Speed Widget Release..." -ForegroundColor Cyan
Write-Host ""

# Clean previous builds
Write-Host "[CLEAN] Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean -c Release
if ($LASTEXITCODE -ne 0) { 
    Write-Host "[ERROR] Clean failed!" -ForegroundColor Red
    exit 1 
}

# Restore dependencies
Write-Host "[RESTORE] Restoring dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) { 
    Write-Host "[ERROR] Restore failed!" -ForegroundColor Red
    exit 1 
}

# Build Release
Write-Host "[BUILD] Building Release configuration..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) { 
    Write-Host "[ERROR] Build failed!" -ForegroundColor Red
    exit 1 
}

# Publish single-file executable
Write-Host "[PUBLISH] Publishing single-file executable..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true
if ($LASTEXITCODE -ne 0) { 
    Write-Host "[ERROR] Publish failed!" -ForegroundColor Red
    exit 1 
}

# Get the output path
$publishPath = "bin\Release\net9.0-windows\win-x64\publish"
$exePath = "$publishPath\NetworkSpeedWidget.exe"

if (Test-Path $exePath) {
    $fileSize = (Get-Item $exePath).Length / 1MB
    Write-Host ""
    Write-Host "[SUCCESS] Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "[INFO] Location: $exePath" -ForegroundColor Cyan
    Write-Host "[INFO] Size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "[*] Next steps:" -ForegroundColor Yellow
    Write-Host "   1. Test the executable: .\$exePath" -ForegroundColor White
    Write-Host "   2. Create GitHub release and upload the exe" -ForegroundColor White
    Write-Host "   3. Add release notes describing features" -ForegroundColor White
    Write-Host ""
    
    # Open the folder
    Write-Host "[*] Opening publish folder..." -ForegroundColor Cyan
    explorer.exe $publishPath
} else {
    Write-Host "[ERROR] Executable not found!" -ForegroundColor Red
    exit 1
}
