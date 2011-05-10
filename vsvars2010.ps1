$_reg = ("HKCU:\Software", "HKCU:\Software\Wow6432Node", "HKLM:\Software", "HKLM:\Software\Wow6432Node")

function ?? {
    PARAM([object[]] $values)
    $result = $null
    $values | %{ if ($result -eq $null) { $result = $_ } }
    return $result
}

function Prepend-IfExists {
    PARAM(
        [string] $newPath,
        [string] $envVar = "PATH"
    )
    $envPath = ("Env:\" + $envVar)
    $oldPath = get-content $envPath -ea:SilentlyContinue
    if (($newPath -ne $null) -and (test-path $newPath)) {
        if ($oldPath -ne $null) { $newPath = $newPath + ";" + $oldPath }
        set-content $envPath $newPath
    }
}

$VsInstallDir = ?? ($_reg | %{ (get-itemproperty -ea:SilentlyContinue ($_ + "\Microsoft\VisualStudio\SxS\VS7"))."10.0" })

if (($VsInstallDir -ne $null) -and (test-path $VsInstallDir)) {
    write-host "Setting environment for Microsoft Visual Studio 2010."

    $WindowsSdkDir      = ?? ($_reg | %{ (get-itemproperty -ea:SilentlyContinue ($_ + "\Microsoft\Microsoft SDKs\Windows\v7.0A")).InstallationFolder })
    $VcInstallDir       = ?? ($_reg | %{ (get-itemproperty -ea:SilentlyContinue ($_ + "\Microsoft\VisualStudio\SxS\VC7"))."10.0" })
    $FSharpInstallDir   = ?? ($_reg | %{ (get-itemproperty -ea:SilentlyContinue ($_ + "\Microsoft\VisualStudio\10.0\Setup\F#")).ProductDir })
    $FrameworkDir       = ?? ($_reg | %{ (get-itemproperty -ea:SilentlyContinue ($_ + "\Microsoft\VisualStudio\SxS\VC7")).FrameworkDir32 })
    $FrameworkVersion   = ?? ($_reg | %{ (get-itemproperty -ea:SilentlyContinue ($_ + "\Microsoft\VisualStudio\SxS\VC7")).FrameworkVer32 })
    $Framework35Version = "v3.5"

    if ($WindowsSdkDir -ne $null) {
        Prepend-IfExists ($WindowsSdkDir + "bin\NET FX 4.0 Tools") "PATH"
        Prepend-IfExists ($WindowsSdkDir + "bin")                  "PATH"
        Prepend-IfExists ($WindowsSdkDir + "include")              "INCLUDE"
        Prepend-IfExists ($WindowsSdkDir + "lib")                  "LIB"
    }

    Prepend-IfExists ($VsInstallDir + "Team Tools\Performance Tools")   "PATH"
    Prepend-IfExists ($env:ProgramFiles + "\HTML Help Workshop")        "PATH"
    Prepend-IfExists (${env:ProgramFiles(x86)} + "\HTML Help Workshop") "PATH"
    Prepend-IfExists ($VcInstallDir + "VCPackages")                     "PATH"
    Prepend-IfExists ($FrameworkDir + $Framework35Version)              "PATH"
    Prepend-IfExists ($FrameworkDir + $FrameworkVersion)                "PATH"
    Prepend-IfExists ($VsInstallDir + "Common7\Tools")                  "PATH"
    Prepend-IfExists ($VcInstallDir + "bin")                            "PATH"
    Prepend-IfExists ($VsInstallDir + "Common7\IDE")                    "PATH"
    Prepend-IfExists ($VsInstallDir + "VSTSDB\Deploy")                  "PATH"
    Prepend-IfExists ($FSharpInstallDir)                                "PATH"

    Prepend-IfExists ($VcInstallDir + "ATLMFC\INCLUDE") "INCLUDE"
    Prepend-IfExists ($VcInstallDir + "INCLUDE")        "INCLUDE"

    Prepend-IfExists ($VcInstallDir + "ATLMFC\LIB")        "LIBPATH"
    Prepend-IfExists ($VcInstallDir + "LIB")               "LIBPATH"
    Prepend-IfExists ($FrameworkDir + $Framework35Version) "LIBPATH"
    Prepend-IfExists ($FrameworkDir + $FrameworkVersion)   "LIBPATH"
}