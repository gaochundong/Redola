Commands
------------
nuget setApiKey xxx-xxx-xxxx-xxxx

nuget push .\packages\Redola.1.0.0.0.nupkg
nuget pack ..\Redola\Redola\Redola.csproj -IncludeReferencedProjects -Symbols -Build -Prop Configuration=Release -OutputDirectory ".\packages"
