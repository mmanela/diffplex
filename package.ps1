dotnet build -c Release

push-location DiffPlex

dotnet pack -c Release

pop-location


push-location DiffPlex.Wpf

dotnet pack -c Release

pop-location