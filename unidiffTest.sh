#!/bin/bash

# Create a temporary file for the old version
TEMP_FILE=$(mktemp)

FILE_PATH="README.md"

# Get the previous version of Program.cs from git history
git show HEAD~20:$FILE_PATH > "$TEMP_FILE"

# Run the ConsoleRunner with file mode
dotnet run --project DiffPlex.ConsoleRunner file "$TEMP_FILE" "$FILE_PATH"

# Clean up the temporary file
rm "$TEMP_FILE"