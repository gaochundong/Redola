Commands
------------
nuget setApiKey xxx-xxx-xxxx-xxxx

nuget push .\packages\Redola.Rpc.0.1.0.0.nupkg
nuget pack ..\Redola\Redola\Redola.Rpc.csproj -IncludeReferencedProjects -Symbols -Build -Prop Configuration=Release -OutputDirectory ".\packages"
