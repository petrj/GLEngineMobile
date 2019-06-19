$scriptPath = $PSScriptRoot
cd $PSScriptRoot

foreach ($folder in `
    @(
        ".vs",
        "LabyrinthDemo\bin",		
        "LabyrinthDemo\obj",
        "SpaceDemo\bin",
        "SpaceDemo\obj",		
		"GLEngineMobile\bin",
        "GLEngineMobile\obj",		
        "LoggerService\bin",
        "LoggerService\obj"
     ))
{
    $fullPath = [System.IO.Path]::Combine($scriptPath,$folder)
    if (-not $fullPath.EndsWith("\"))
    {
            $fullPath += "\"
    }

    if (Test-Path -Path $fullPath)
    {
        Remove-Item -Path $fullPath -Recurse -Force -Verbose
    }
}
