#!/bin/bash

#install zip on debian OS, since microsoft/dotnet container doesn't have zip by default
if [ -f /etc/debian_version ] && ! [ -x "$(command -v zip)" ]
then
  apt -qq update
  apt -qq -y install zip
fi

dotnet tool install -g Amazon.Lambda.Tools --framework net6.0
dotnet tool restore
dotnet lambda package --configuration Release --framework net6.0 --output-package out/ShopifyMiddleware.zip
exit 0
