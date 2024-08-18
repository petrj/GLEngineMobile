
function Write-Graphics
{
    param
    (
       $ImageFileName,
       $WorkingDirectory,
       $DefTable,
       $ConvertPath
    )
    process
    {
        foreach ($subFolder in $DefTable.keys)
        {
            $size = $DefTable[$subFolder]
            $folder = Join-Path -Path $WorkingDirectory -ChildPath $subFolder            
            
            $fname = Join-Path -Path $folder -ChildPath ([System.IO.Path]::GetFileName($ImageFileName))
            Write-Host ($ImageFileName + " => " + $fname + " : " + $size)

            if (-not (Test-Path -Path $folder))
            {
                New-Item -Path $folder  -ItemType Directory -Force -Verbose
            }            

            & $ConvertPath $ImageFileName -resize $size -size $size $fname
        }
    }
}

$icon_folder_size = @{}
$icon_folder_size.Add("mipmap-mdpi","80x80")
$icon_folder_size.Add("mipmap-hdpi","120x120")
$icon_folder_size.Add("mipmap-xhdpi","160x160")
$icon_folder_size.Add("mipmap-xxhdpi","240x240")
$icon_folder_size.Add("mipmap-xxxhdpi","320x320")

$banner_folder_size = @{}
$banner_folder_size.Add("mipmap-mdpi","160x90")
$banner_folder_size.Add("mipmap-hdpi","240x135")
$banner_folder_size.Add("mipmap-xhdpi","320x180")
$banner_folder_size.Add("mipmap-xxhdpi","480x270")
$banner_folder_size.Add("mipmap-xxxhdpi","640x360")

Write-Graphics `
    -ImageFileName "C:\temp\icons\ic_launcher.png" `
    -WorkingDirectory "C:\temp\icons\" `
    -DefTable $icon_folder_size `
    -ConvertPath "C:\Program Files\ImageMagick\magick.exe"

Write-Graphics `
    -ImageFileName "C:\temp\icons\banner.png" `
    -WorkingDirectory "C:\temp\icons\" `
    -DefTable $banner_folder_size `
    -ConvertPath "C:\Program Files\ImageMagick\magick.exe"
    
