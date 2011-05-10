properties {
  $baseDir = Resolve-Path .
  $buildNumber = '1.2.0.0'
  
  $configuration = "debug"
  $xUnit = Resolve-Path .\3rdParty\xUnit.net\xunit.console.clr4.exe
  $nugetDir = "$baseDir\_nuget"
  
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

task Build-Package -depends Build-Solution, Prepare-NuGet {
   exec { &"$baseDir\3rdParty\NuGet\NuGet.exe" pack "$nugetDir\DiffPlex.nuspec" >> $NULL }
   clean $nugetDir
 
}

task Prepare-NuGet {
    $nuget40 = "$nugetDir\lib\Net40"
    $nugetSL = "$nugetDir\lib\SL4"
    $nuspec = "$baseDir\DiffPlex.nuspec"
    
    clean $nugetDir
    create $nugetDir, $nugetSL, $nuget40
    
    copy-item "$baseDir\License.txt", $nuspec -destination $nugetDir
    copy-item "$baseDir\DiffPlex\bin\$configuration\DiffPlex.*" -destination $nuget40
    copy-item "$baseDir\DiffPlex.Silverlight\bin\$configuration\DiffPlex.*.dll" -destination $nugetSL
    copy-item "$baseDir\DiffPlex.Silverlight\bin\$configuration\DiffPlex.*.pdb" -destination $nugetSL
    
    $version = new-object -TypeName System.Version -ArgumentList $buildNumber
    regex-replace "$nugetDir\DiffPlex.nuspec" '(?m)@Version@' $version.ToString(3)
}

# Help 
task ? -Description "Help information" {
	Write-Documentation
}

function create([string[]]$paths) {
  foreach ($path in $paths) {
    new-item -path $path -type directory | out-null
  }
}

function regex-replace($filePath, $find, $replacement) {
    $regex = [regex] $find
    $content = [System.IO.File]::ReadAllText($filePath)
    
    Assert $regex.IsMatch($content) "Unable to find the regex '$find' to update the file '$filePath'"
    
    [System.IO.File]::WriteAllText($filePath, $regex.Replace($content, $replacement))
}

function clean([string[]]$paths) {
	foreach ($path in $paths) {
		remove-item -force -recurse $path -ErrorAction SilentlyContinue
	}
}

function roboexec([scriptblock]$cmd) {
    & $cmd | out-null
    if ($lastexitcode -eq 0) { throw "No files were copied for command: " + $cmd }
}
