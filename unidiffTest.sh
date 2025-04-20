#!/bin/bash

# Create a temporary file for the old version
TEMP_FILE=$(mktemp)

# Get the previous version of Program.cs from git history
git show HEAD~1:DiffPlex.ConsoleRunner/Program.cs > "$TEMP_FILE"

# Run the ConsoleRunner with file mode
dotnet run --project DiffPlex.ConsoleRunner file "$TEMP_FILE" "DiffPlex.ConsoleRunner/Program.cs"

# Clean up the temporary file
rm "$TEMP_FILE"