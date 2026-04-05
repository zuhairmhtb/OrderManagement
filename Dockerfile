FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:3fcf6f1e809c0553f9feb222369f58749af314af6f063f389cbd2f913b4ad556 AS build
WORKDIR /App

# Copy csproj files first for better layer caching
COPY *.slnx ./
COPY OrderManagement.Web/OrderManagement.Web.csproj ./OrderManagement.Web/
COPY OrderManagement.Database/OrderManagement.Database.csproj ./OrderManagement.Database/
COPY OrderManagement.Worker/OrderManagement.Worker.csproj ./OrderManagement.Worker/

# Restore as distinct layers
RUN dotnet restore OrderManagement.Web/OrderManagement.Web.csproj

# Copy everything else
COPY . ./

# Build and publish a release
RUN dotnet publish OrderManagement.Web/OrderManagement.Web.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:b4bea3a52a0a77317fa93c5bbdb076623f81e3e2f201078d89914da71318b5d8
WORKDIR /App

# Expose the port the app runs on
EXPOSE 5115

COPY --from=build /App/out .

ENTRYPOINT ["dotnet", "OrderManagement.Web.dll"]