cd $PSScriptRoot

$convert = "C:\Program Files (x86)\ImageMagick\convert.exe"
$ResourcesFolder = "C:\GLEngineMobile\Easy3DLabyrinth\Resources"
$SourceImage = "C:\GLEngineMobile\Graphics\Easy3DLabyrinth.png";

$tmpFileName = [System.IO.Path]::GetTempFileName();

$SourceImageName = [System.IO.Path]::GetFileName($SourceImage)

& $convert -size 128x128 $SourceImage -resize 128x128 $ResourcesFolder\drawable-nodpi\Easy3DLabyrinth.png

# mipmap-hdpi

& $convert -size 58x58 $SourceImage -resize 58x58 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 72x72 $ResourcesFolder\mipmap-hdpi\ic_launcher.png

& $convert  -size 80x80  $SourceImage -resize 80x80 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 162x162 $ResourcesFolder\mipmap-hdpi\ic_launcher_foreground.png

& $convert -size 72x72 $SourceImage -resize 72x72 $ResourcesFolder\mipmap-hdpi\ic_launcher_round.png

# mipmap-mdpi

& $convert -size 38x38 $SourceImage -resize 38x38 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 44x44 $ResourcesFolder\mipmap-mdpi\ic_launcher.png

& $convert  -size 50x50  $SourceImage -resize 50x50 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 108x108 $ResourcesFolder\mipmap-mdpi\ic_launcher_foreground.png

& $convert -size 48x48 $SourceImage -resize 48x48 $ResourcesFolder\mipmap-mdpi\ic_launcher_round.png

# mipmap-xhdpi

& $convert -size 78x78 $SourceImage -resize 78x78 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 96x96 $ResourcesFolder\mipmap-xhdpi\ic_launcher.png

& $convert  -size 105x105  $SourceImage -resize 105x105 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 216x216 $ResourcesFolder\mipmap-xhdpi\ic_launcher_foreground.png

& $convert -size 96x96 $SourceImage -resize 96x96 $ResourcesFolder\mipmap-xhdpi\ic_launcher_round.png


# mipmap-xxhdpi

& $convert -size 115x115 $SourceImage -resize 115x115 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 144x144 $ResourcesFolder\mipmap-xxhdpi\ic_launcher.png

& $convert  -size 150x150  $SourceImage -resize 150x150 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 324x324 $ResourcesFolder\mipmap-xxhdpi\ic_launcher_foreground.png

& $convert -size 144x144 $SourceImage -resize 144x144 $ResourcesFolder\mipmap-xxhdpi\ic_launcher_round.png


# mipmap-xxxhdpi

& $convert -size 150x150 $SourceImage -resize 150x150 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 192x192 $ResourcesFolder\mipmap-xxxhdpi\ic_launcher.png

& $convert  -size 200x200  $SourceImage -resize 200x200 $tmpFileName
& $convert $tmpFileName -background none -gravity center -extent 432x432 $ResourcesFolder\mipmap-xxxhdpi\ic_launcher_foreground.png

& $convert -size 192x192 $SourceImage -resize 192x192 $ResourcesFolder\mipmap-xxxhdpi\ic_launcher_round.png




