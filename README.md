# TextureSplitter
Splits textures in 4 or 2 automaticly just drop files or folders into the exe. Only works with PNG files

This works by using monogame.

If Square it splits in 4 even parts.

If Width is two times the Height, splits in 2 even parts.

This project uses [Monogame 3.7.1](https://github.com/MonoGame/MonoGame/releases/tag/v3.7.1)

To build on linux:
```sh
sudo apt update
sudo apt-get --assume-yes install nuget mono-complete mono-devel gtk-sharp3 ffmpeg
echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | sudo debconf-set-selections
sudo apt-get --assume-yes install ttf-mscorefonts-installer
wget https://github.com/MonoGame/MonoGame/releases/download/v3.7.1/monogame-sdk.run
chmod +x monogame-sdk.run
sudo ./monogame-sdk.run --noexec --keep --target ./monogame
cd monogame
echo Y | sudo ./postinstall.sh
cd ..  
nuget restore
msbuild
```
