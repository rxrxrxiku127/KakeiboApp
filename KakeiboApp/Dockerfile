# =====================================================
# ビルドステージ
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o out

# =====================================================
# 実行ステージ
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime

WORKDIR /app

COPY --from=build /app/out ./

RUN mkdir -p /data

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "KakeiboApp.dll"]