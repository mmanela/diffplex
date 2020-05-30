dotnet build -c Release

push-location DiffPlex

dotnet pack -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg

pop-location


push-location DiffPlex.Wpf

dotnet pack -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg

pop-location