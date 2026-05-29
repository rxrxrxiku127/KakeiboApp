# =====================================================
# ビルドステージ
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

# サブフォルダのプロジェクトファイルをコピー
COPY KakeiboApp/*.csproj ./KakeiboApp/
RUN dotnet restore ./KakeiboApp/KakeiboApp.csproj

# 残りのソースコードをコピー
COPY . ./

# リリースビルドを実行
RUN dotnet publish ./KakeiboApp/KakeiboApp.csproj -c Release -o out

# =====================================================
# 実行ステージ
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/out ./

# SQLiteのデータ保存先
RUN mkdir -p /data

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/data/kakeibo.db"

EXPOSE 8080

ENTRYPOINT ["dotnet", "KakeiboApp.dll"]