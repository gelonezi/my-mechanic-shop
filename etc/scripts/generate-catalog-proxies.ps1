$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$hostUrl = "https://localhost:44390"

# abp run starts every application at once, so wait for the host to be healthy
# before asking it for its API definition.
$deadline = (Get-Date).AddMinutes(5)
while ((curl.exe -sk -o NUL -w "%{http_code}" "$hostUrl/health-status") -ne "200") {
    if ((Get-Date) -gt $deadline) {
        [Console]::Error.WriteLine("Host not healthy at $hostUrl after 5 minutes; Catalog proxies not generated.")
        exit 1
    }
    Start-Sleep -Seconds 2
}

# Run from the module's Angular workspace. Every flag is load-bearing;
# see .claude/rules/project/auto-api-controllers.md.
Push-Location (Join-Path $scriptRoot "../../modules/MyMechanicShop.Catalog/angular")
try {
    abp generate-proxy -t ng -m catalog -s dev-app --target catalog -a Catalog
    $exitCode = $LASTEXITCODE

    # The generator writes some files CRLF and some LF. .gitattributes keeps proxy folders
    # LF, so normalize them; otherwise an unchanged API still shows up in `git status`.
    Get-ChildItem "projects/catalog/src/lib/proxy" -Recurse -File | ForEach-Object {
        $text = [System.IO.File]::ReadAllText($_.FullName)
        $lf = $text -replace "`r`n", "`n"
        if ($lf -ne $text) {
            [System.IO.File]::WriteAllText($_.FullName, $lf)
        }
    }
}
finally {
    Pop-Location
}

exit $exitCode
