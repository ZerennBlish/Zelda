# copy-for-claude.ps1
# Stages all Zerenn project-knowledge DOCS into a flat folder
# for Claude.ai project knowledge upload. Run from anywhere — paths are absolute.
#
# NOTE: Scripts (.cs files) are NOT staged here. They go stale between sessions.
# Opus uses Desktop Commander to read live scripts on demand instead.
#
# Maintenance:
#   - When adding, removing, or renaming a TOP-LEVEL config/doc file, update
#     the $rootFiles list below.
#   - Docs/*.md files are auto-discovered.

$source = "C:\Zelda"
$dest   = "C:\Users\baldy\OneDrive\Desktop\BaldGuy&CompanyGames\ZerennZelda\Docs"

# --- Setup ---

# Wipe destination so deleted/renamed files don't linger as stale uploads.
if (Test-Path $dest) {
    Remove-Item "$dest\*" -Force
} else {
    New-Item -ItemType Directory -Path $dest -Force | Out-Null
}

# --- Root config files (hand-maintained) ---

$rootFiles = @(
    "CLAUDE.md",
    "AGENTS.md",
    "GEMINI.md",
    "CONVENTIONS.md",
    "README.md"
)

foreach ($f in $rootFiles) {
    $src = Join-Path $source $f
    if (Test-Path $src) {
        Copy-Item $src "$dest\$f" -Force
    } else {
        Write-Warning "Missing root file: $f"
    }
}

# --- AI documentation (auto-discovered: all .md in Docs/) ---

$docsDir = Join-Path $source "Docs"
if (Test-Path $docsDir) {
    Get-ChildItem -Path $docsDir -Filter "*.md" -File | ForEach-Object {
        Copy-Item $_.FullName "$dest\$($_.Name)" -Force
    }
}

# --- Summary ---

$count = (Get-ChildItem $dest -File).Count
Write-Host "Copied $count files to $dest"
