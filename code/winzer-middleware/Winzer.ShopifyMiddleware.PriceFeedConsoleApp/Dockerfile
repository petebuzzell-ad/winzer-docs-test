FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /app

# Copy everything
COPY ./out .

ENTRYPOINT ["dotnet", "Winzer.ShopifyMiddleware.PriceFeedConsoleApp.dll"]
