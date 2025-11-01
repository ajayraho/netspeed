# GitHub Release Instructions

## Step 1: Prepare Your Repository

1. Make sure all files are committed:
```powershell
git add .
git commit -m "Release v1.0.0"
git push origin main
```

## Step 2: Create a Release on GitHub

1. Go to your repository on GitHub
2. Click on "Releases" (right sidebar)
3. Click "Create a new release"

### Release Details:

**Tag version**: `v1.0.0`

**Release title**: `Network Speed Widget v1.0.0 - Initial Release`

**Description**: Copy content from `RELEASE_NOTES.md`

## Step 3: Upload the Executable

1. Click "Attach binaries by dropping them here or selecting them"
2. Upload: `bin\Release\net9.0-windows\win-x64\publish\NetworkSpeedWidget.exe`
3. Optionally rename to: `NetworkSpeedWidget-v1.0.0-win-x64.exe`

## Step 4: Publish Release

1. Check "Set as the latest release"
2. Click "Publish release"

## Your Release is Ready! 🎉

Users can now download the exe from:
`https://github.com/yourusername/NetworkSpeedWidget/releases/latest`

## Additional Files to Include (Optional)

You might also want to upload a ZIP file with:
- NetworkSpeedWidget.exe
- README.md
- LICENSE
- RELEASE_NOTES.md

Create the ZIP:
```powershell
$files = @(
    "bin\Release\net9.0-windows\win-x64\publish\NetworkSpeedWidget.exe",
    "README.md",
    "LICENSE",
    "RELEASE_NOTES.md"
)

Compress-Archive -Path $files -DestinationPath "NetworkSpeedWidget-v1.0.0-win-x64.zip" -Force
```

## Post-Release Checklist

- [ ] Update README.md with download link
- [ ] Test download link works
- [ ] Share on social media / Reddit
- [ ] Monitor GitHub Issues for bug reports
- [ ] Plan next version features

## Badge for README

Add this to your README.md to show the latest release:

```markdown
[![GitHub release](https://img.shields.io/github/v/release/yourusername/NetworkSpeedWidget)](https://github.com/yourusername/NetworkSpeedWidget/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/yourusername/NetworkSpeedWidget/total)](https://github.com/yourusername/NetworkSpeedWidget/releases)
```
