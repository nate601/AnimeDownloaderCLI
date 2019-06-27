dotnet publish -r win-x64 --self-contained true -c Release
dotnet publish -r linux-x64 --self-contained true -c Release
dotnet publish -r osx-x64 --self-contained true -c Release
cd bin
cd Release
cd netcoreapp2.2
warp-packer -a windows-x64 -i win-x64\publish\ -e AnimeDownloaderCLI.exe -o AnimeDownloaderCLI.exe
warp-packer -a linux-x64 -i linux-x64\publish\ -e AnimeDownloaderCLI -o AnimeDownloaderCLILinux
warp-packer -a osx-x64 -i osx-x64\publish\ -e AnimeDownloaderCLI -o AnimeDownloaderCLIOsx
7z a win-x64 AnimeDownloaderCLI.exe
7z a linux-x64 AnimeDownloaderCLILinux
7z a osx-x64 AnimeDownloaderCLIOsx
dir
cd ..
cd ..
cd ..