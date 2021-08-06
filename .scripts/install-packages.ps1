[CmdletBinding()]
Param(
    [string]$Version,
    [string]$ApiKey
)

if (-Not $Version) {
    Write-Host -ForegroundColor Red "Please specify the version to publish"
    Write-Host -ForegroundColor Cyan -NoNewLine "USAGE: "
    Write-Host "install-packages.ps1 -version <version>"
    Write-Host -ForegroundColor Yellow "Existing packages listed below:"
    nuget list -source $NuGetSource -prerelease
    exit -1
}

if(-Not $ApiKey) {
    $ApiKey = "********************************************"
}

#$NuGetSource = "https://nuget.pkg.github.com/prathimanm/index.json"
$NuGetSource = "https://api.nuget.org/v3/index.json"

dotnet build -c Release

Function Publish-Package-Tool {
    Param ([string]$Folder, [string]$Name)
    Write-Host -ForegroundColor Magenta "Packing and publishing $Folder-$Name package"
    dotnet pack ./$Folder/$Name/$Name.csproj -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -c Release-Tool /p:Version=$Version
    dotnet nuget push ./$Folder/$Name/bin/Release-Tool/dotnet-nswagen.$Version.nupkg -s $NuGetSource --api-key $ApiKey
}

Function Publish-Package {
    Param ([string]$Folder, [string]$Name)
    Write-Host -ForegroundColor Magenta "Packing and publishing $Folder-$Name package"
    dotnet pack ./$Folder/$Name/$Name.csproj -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -c Release /p:Version=$Version
    dotnet nuget push ./$Folder/$Name/bin/Release/dotnet-nswagen.$Version.nupkg -s $NuGetSource --api-key $ApiKey
}


   #Publish-Package "src" "NSwagen.Annotations"
   Publish-Package-Tool "src" "NSwagen"
   
   
   


