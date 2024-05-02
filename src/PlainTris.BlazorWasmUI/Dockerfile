FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/PlainTris.BlazorWasmUI/PlainTris.BlazorWasmUI.csproj", "src/PlainTris.BlazorWasmUI/"]
RUN dotnet restore "src/PlainTris.BlazorWasmUI/PlainTris.BlazorWasmUI.csproj"
COPY . .
WORKDIR "/src/src/PlainTris.BlazorWasmUI"
RUN dotnet build "PlainTris.BlazorWasmUI.csproj" -c "$configuration" -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "PlainTris.BlazorWasmUI.csproj" -c "$configuration" -o /app/publish /p:UseAppHost=false

FROM rtsp/lighttpd AS final
EXPOSE 80
WORKDIR /var/www/html
COPY --from=publish /app/publish/wwwroot .

