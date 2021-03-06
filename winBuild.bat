
dotnet publish -r win-x64 --self-contained true -c Release
dotnet publish -r linux-x64 --self-contained true -c Release
dotnet publish -r osx-x64 --self-contained true -c Release
dotnet publish -r win-x64 --self-contained false -c Release -o bin\Release\netcoreapp3.1\win-x64-dependent\publish

windows-x64.warp-packer.exe -a windows-x64 -i bin\Release\netcoreapp3.1\win-x64\publish -e AnimeDownloaderCLI.exe -o AnimeDownloaderCLI.exe
windows-x64.warp-packer.exe -a linux-x64 -i bin\Release\netcoreapp3.1\linux-x64\publish\ -e AnimeDownloaderCLI -o AnimeDownloaderCLILinux
windows-x64.warp-packer.exe -a macos-x64 -i bin\Release\netcoreapp3.1\osx-x64\publish\ -e AnimeDownloaderCLI -o AnimeDownloaderCLIOsx
windows-x64.warp-packer.exe -a windows-x64 -i bin\Release\netcoreapp3.1\win-x64-dependent\publish -e AnimeDownloaderCLI.exe -o AnimeDownloaderCLIDep.exe

powershell Compress-Archive -update AnimeDownloaderCLI.exe windows.zip
powershell Compress-Archive -update AnimeDownloaderCLILinux linux.zip
powershell Compress-Archive -update AnimeDownloaderCLIOsx macos.zip
powershell Compress-Archive -update AnimeDownloaderCLIDep.exe windows-framework-dependent.zip


del /q bin\Release\netcoreapp3.1\win-x64
del /q bin\Release\netcoreapp3.1\linux-x64
del /q bin\Release\netcoreapp3.1\macos-x64
del /q bin\Release\netcoreapp3.1\win-x64-dependent

del /q AnimeDownloaderCLI.exe
del /q AnimeDownloaderCLILinux
del /q AnimeDownloaderCLIOsx
del /q AnimeDownloaderCLIDep.exe
