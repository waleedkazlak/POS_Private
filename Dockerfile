FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files for optimal layer caching
COPY ["POS_Private.Domain/POS_Private.Domain.csproj", "POS_Private.Domain/"]
COPY ["POS_Private.Application/POS_Private.Application.csproj", "POS_Private.Application/"]
COPY ["POS_Private.Infrastructure/POS_Private.Infrastructure.csproj", "POS_Private.Infrastructure/"]
COPY ["POS_Private.Presentation/POS_Private.Presentation.csproj", "POS_Private.Presentation/"]

RUN dotnet restore "POS_Private.Presentation/POS_Private.Presentation.csproj"

# Copy remaining source code and publish
COPY . .
WORKDIR "/src/POS_Private.Presentation"
RUN dotnet publish "POS_Private.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "POS_Private.Presentation.dll"]
