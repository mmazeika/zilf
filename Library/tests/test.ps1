$zilfPath = "..\..\Zilf\bin\Debug\zilf.exe"
$zapfPath = "..\..\Zapf\bin\Debug\zapf.exe"
$czlrPath = ".\ConsoleZLR.exe"
$includeDir = ".."

function Compile-Zil {
    param ([string]$SrcFile = $(throw "SrcFile parameter is required."))
    & $zilfPath -ip $includeDir $SrcFile >$null 2>&1
    return $LASTEXITCODE -eq 0
}

function Assemble-Zap {
    param ([string]$SrcFile = $(throw "SrcFile parameter is required."))
    & $zapfPath $SrcFile >$null 2>&1
    return $LASTEXITCODE -eq 0
}

function Run-Zcode {
    param ([string]$StoryFile = $(throw "StoryFile parameter is required."))
    & $czlrPath -nowait -dumb $StoryFile
}

function Run-Test {
    param ($TestName = $(throw "TestName parameter is required."),
           [switch]$Silent = $false)
    
    $testFile = ".\test-" + $TestName + ".zil"
    if (Compile-Zil $testFile) {
        $zapFile = [io.path]::ChangeExtension($testFile, ".zap")
        if (Assemble-Zap $zapFile) {
            $storyFile = [io.path]::ChangeExtension($zapFile, ".z3")
            $output = $(Run-Zcode $storyFile)
            if ($output -match "^PASS$") {
                return $true
            } elseif (!$Silent) {
                Write-Host ($output | Out-String)
                return
            }
        }
    }
    return $false
}

Set-Alias test Run-Test

function Get-TestNames {
    dir test-*.zil | foreach { $_.Name -replace '^test-(.*)\.zil$', '$1' }
}

function Test-All {
    $testNames = Get-TestNames
    $completed = 0
    
    foreach ($t in $testNames) {
        Write-Progress -Activity "Running tests" -Status $t -PercentComplete (($completed) * 100 / $testNames.Count)
    
        if (Run-Test $t -Silent) {$status = "Pass"} else {$status = "Fail"}
        $completed++
        
        $hash = @{Name=$t; Status=$status}
        New-Object PSObject -Property $hash
    }
}
