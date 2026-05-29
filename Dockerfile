# =====================================================
# ビルドステージ
# .NET SDKイメージを使ってアプリをビルドする
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# 作業ディレクトリを設定
WORKDIR /app

# プロジェクトファイルをコピーしてパッケージを復元
# ※先にこれだけコピーすることでDockerのキャッシュを活用できる
COPY *.csproj ./
RUN dotnet restore

# 残りのソースコードをコピー
COPY . ./

# リリースビルドを実行
RUN dotnet publish -c Release -o out

# =====================================================
# 実行ステージ
# ASP.NET Coreランタイムイメージを使って軽量化する
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# 作業ディレクトリを設定
WORKDIR /app

# ビルドステージの成果物をコピー
COPY --from=build /app/out ./

# =====================================================
# SQLiteのデータ保存先ディレクトリを作成
# Renderの永続ストレージにマウントするために使用
# =====================================================
RUN mkdir -p /data

# 環境変数の設定
# Renderは8080番ポートを使用する
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# SQLiteのDBファイルをdataフォルダに保存するよう設定
ENV ConnectionStrings__DefaultConnection="Data Source=/data/kakeibo.db"

# 8080番ポートを公開
EXPOSE 8080

# アプリを起動
ENTRYPOINT ["dotnet", "KakeiboApp.dll"]