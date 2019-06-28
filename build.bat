@echo off

REM Nathan Button

REM The build.bat creates a build on (win/linux/osx)-x64, packs it into a
REM single executable using warp-packer, and then zips it into a 7z file
REM using 7zip.

REM To use this script the build computer must have the dotnet core sdk,
REM warp-packer, and 7zip installed.

REM After running this command the results will be in
REM bin/release/netcoreapp2.2

dotnet publish -r win-x64 --self-contained true -c Release
dotnet publish -r linux-x64 --self-contained true -c Release
dotnet publish -r osx-x64 --self-contained true -c Release
cd bin
cd Release
cd netcoreapp2.2
warp-packer -a windows-x64 -i win-x64\publish\ -e AnimeDownloaderCLI.exe -o AnimeDownloaderCLI.exe
warp-packer -a linux-x64 -i linux-x64\publish\ -e AnimeDownloaderCLI -o AnimeDownloaderCLILinux
warp-packer -a macos-x64 -i osx-x64\publish\ -e AnimeDownloaderCLI -o AnimeDownloaderCLIOsx
7z a win-x64 AnimeDownloaderCLI.exe
7z a linux-x64 AnimeDownloaderCLILinux
7z a osx-x64 AnimeDownloaderCLIOsx
rm -rf win-x64
rm -rf linux-x64
rm -rf osx-x64
rm AnimeDownloaderCLI.exe
rm AnimeDownloaderCLILinux
rm AnimeDownloaderCLIOsx
cd ..
cd ..
cd ..
dotnet publish -r win-x64 --self-contained false -c Release
cd bin
cd Release
cd netcoreapp2.2
warp-packer -a windows-x64 -i win-x64\publish\ -e AnimeDownloaderCLI.exe -o AnimeDownloaderCLI.exe
7z a win-x64-framework-dependent AnimeDownloader.exe
rm AnimeDownloader.exe
cd ..
cd ..
cd ..
echo "Completed."
