FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS conf
WORKDIR /app
ENV TZ=America/Sao_Paulo
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS dev
WORKDIR build
COPY . ./

RUN dotnet restore
RUN dotnet publish -c Release -o /app/dist --no-restore

FROM conf as deploy
WORKDIR /app
COPY --from=dev /app/dist .

ENTRYPOINT ["dotnet", "Umbrella.Api.dll"]

# docker build -t granto-backend -f src/Umbrella.Api/Dockerfile .
# docker run --rm -it -p 8080:80 --name teste granto-backend
# docker tag granto-backend granto.azurecr.io/granto-backend
# docker push granto.azurecr.io/granto-backend
