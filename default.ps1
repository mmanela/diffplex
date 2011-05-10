properties {
  $baseDir = Resolve-Path .
  $configuration = "debug"
  $xUnit = Resolve-Path .\3rdParty\xUnit.net\xunit.console.clr4.exe

    # Import environment variables for Visual Studio
  if (test-path ("vsvars2010.ps1")) { 
    . vsvars2010.ps1 
    }
}

# Aliases
task Default -depends Run-Build
task Build -depends Run-Build
task Clean -depends Clean-Solution

# Build Tasks
task Run-Build -depends  Clean-Solution, Build-Solution, Run-UnitTests

task Clean-Solution {
    exec { msbuild DiffPlex.sln /t:Clean /v:quiet }
}

task Build-Solution {
    exec { msbuild DiffPlex.sln /maxcpucount /t:Build /v:Minimal /p:Configuration=$configuration }
}

task Run-UnitTests {
    exec { & $xUnit "Facts.DiffPlex\bin\$configuration\Facts.DiffPlex.dll" }
    exec { & $xUnit "Facts.WebDiffer\bin\$configuration\Facts.WebDiffer.dll" }
}


# Help 
task ? -Description "Help information" {
	Write-Documentation
}

function roboexec([scriptblock]$cmd) {
    & $cmd | out-null
    if ($lastexitcode -eq 0) { throw "No files were copied for command: " + $cmd }
}
