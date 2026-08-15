#!/bin/bash

dotnet format whitespace \
    --no-restore

dotnet format style \
    --no-restore

dotnet format analyzers \
    --no-restore
