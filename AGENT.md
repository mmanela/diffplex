# DiffPlex Development Guide

## Build/Test Commands
- Build: `dotnet build`
- Test all: `dotnet test`
- Test single project: `dotnet test Facts.DiffPlex`
- Test specific class: `dotnet test Facts.DiffPlex --filter "FullyQualifiedName~InlineDiffBuilderFacts"`
- Test specific method: `dotnet test --filter "FullyQualifiedName~ClassName.MethodName"`
- Restore packages: `dotnet restore`

## Code Style Guidelines
- **Classes/Interfaces**: PascalCase (interfaces with "I" prefix)
- **Methods/Properties**: PascalCase
- **Fields/Parameters/Variables**: camelCase
- **Constants**: PascalCase
- Use expression-bodied members for simple getters
- Modern C# collection initialization: `data.Pieces = [];`
- Null checks with `ArgumentNullException` for public APIs
- Singleton pattern with static `Instance` properties

## Test Conventions
- Test classes: `*Facts` suffix (e.g., `DifferFacts`)
- Use xUnit with `[Fact]` attributes
- Nested classes to organize related tests
- Descriptive test names: `Will_throw_if_parameter_is_null`
- Use Moq for mocking dependencies

## Error Handling
- Throw `ArgumentNullException` for null parameters in public APIs
- Use descriptive parameter names in exceptions
