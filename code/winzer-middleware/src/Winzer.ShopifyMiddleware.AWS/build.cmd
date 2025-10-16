dotnet tool install -g Amazon.Lambda.Tools --framework net6.0
dotnet tool restore
dotnet lambda package --configuration Release --framework net6.0 --output-package out/ShopifyMiddleware.zip
