#!/bin/bash

dotnet publish -c Release -o out
rm -rf out/appsettings.local.json
exit 0
