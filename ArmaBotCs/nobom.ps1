Get-ChildItem -Path . -Recurse -File | ForEach-Object {
    $file = $_.FullName

    # Read the first 3 bytes to check for UTF-8 BOM (0xEF,0xBB,0xBF)
    $fs = [System.IO.File]::Open($file, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read)
    $bom = New-Object byte[] 3
    $fs.Read($bom, 0, 3) | Out-Null
    $fs.Close()

    if ($bom[0] -eq 0xEF -and $bom[1] -eq 0xBB -and $bom[2] -eq 0xBF) {
        Write-Host "Converting $file from UTF-8-BOM to UTF-8..."

        # Read file as UTF8 with BOM, then write as UTF8 without BOM
        $content = Get-Content -Path $file -Raw -Encoding UTF8
        $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllText($file, $content, $utf8NoBom)
    }
}