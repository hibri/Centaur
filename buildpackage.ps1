.nuget\nuget update -Self
.nuget\nuget restore
.nuget\nuget update Centaur.sln
msbuild Centaur.sln /property:Configuration=Release
.nuget\nuget pack Centaur.nuspec