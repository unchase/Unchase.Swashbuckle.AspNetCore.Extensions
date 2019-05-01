#!/bin/bash
dotnet restore
for path in src/*/project.json; do
    dirname="$(dirname "${path}")"
    dotnet build ${dirname} -c Release
done 
 
dotnet restore
for path in test/*/project.json; do
    dirname="$(dirname "${path}")"
    dotnet build ${dirname} -c Release
done 