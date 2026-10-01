# IBAS Support – Blazor WebApp med Azure CosmosDB

<!-- Gruppe: tilføj jeres navne her -->

## Formål

Projektet er afleveringsopgave **M4.04** i Cloud Computing. Det er en .NET Blazor WebApp til IBAS Cykler, hvor kunder kan oprette supporthenvendelser. Henvendelserne gemmes som JSON-dokumenter i en **Azure CosmosDB NoSQL** database (oprettet i M4.03 ud fra datamodellen i M4.02).

Appen kan:

- **Opret henvendelse** (`/createsupport`): en formular med validering (data annotations), som gemmer i CosmosDB
- **Henvendelser** (`/supportlist`): en tabel med alle henvendelser hentet fra CosmosDB

## Datamodel

Hver henvendelse er ét indlejret dokument (ingen relationer), hvor `category` er partition key:

```json
{
  "id": "3f0c9a8e-...",
  "name": "Jens Hansen",
  "email": "jens@example.com",
  "phone": "12345678",
  "category": "reservedele",
  "description": "Jeg mangler en ny kæde til min IBAS City 3.",
  "createdAt": "2026-10-01T09:15:00"
}
```

Kategorier: `teknisk`, `reservedele`, `forslag`, `forhandler`, `katalog`, `andet`.

## Projektstruktur

| Fil | Beskrivelse |
|-----|-------------|
| `SupportWebApp/Models/SupportMessage.cs` | Modelklasse med data annotations |
| `SupportWebApp/Services/CosmosService.cs` | Forbindelse til CosmosDB (indsæt og hent alle) |
| `SupportWebApp/Components/Pages/CreateSupport.razor` | Side til oprettelse af henvendelser |
| `SupportWebApp/Components/Pages/SupportList.razor` | Side med oversigt over henvendelser |
| `SupportWebApp/Program.cs` | Registrerer `CosmosService` med dependency injection |

> Bemærk: I .NET 8+ Blazor-templaten (`dotnet new blazor`) ligger siderne i `Components/Pages` og layout/navigation i `Components/Layout` i stedet for de gamle mapper `Pages` og `Shared`.

## Opret CosmosDB databasen med az-kommandoer

Kør i Git Bash (på Windows skal partition key path have dobbelt `//`, på Mac enkelt `/`):

```bash
az login

# Registrér CosmosDB resource provider
az provider register --namespace Microsoft.DocumentDB --wait
az provider show --namespace Microsoft.DocumentDB --query registrationState

# Variabler
export RESGRP="IBasSupportRG"
export DBACCOUNT="ibas-db-account-$RANDOM"
export DATABASE="IBasSupportDB"
export CONTAINER="ibassupport"
export LOCATION="germanywestcentral"   # Azure for Students tillader kun visse regioner

# Ressourcegruppe
az group create --name $RESGRP --location $LOCATION

# CosmosDB konto (free tier / Azure for Students)
az cosmosdb create --name $DBACCOUNT --resource-group $RESGRP --enable-free-tier true \
    --locations regionName=$LOCATION

# Database
az cosmosdb sql database create --account-name $DBACCOUNT \
    --resource-group $RESGRP --name $DATABASE

# Container med category som partition key
az cosmosdb sql container create --account-name $DBACCOUNT \
    --resource-group $RESGRP --database-name $DATABASE \
    --name $CONTAINER --partition-key-path "//category"
```

## Kør applikationen lokalt

Connection string ligger **ikke** i repo'et (det er public). Den gemmes lokalt med `dotnet user-secrets`:

```bash
cd SupportWebApp

# Hent connection string og gem den som user-secret
dotnet user-secrets set "CosmosDb:ConnectionString" "$(az cosmosdb keys list \
    --name $DBACCOUNT --resource-group $RESGRP --type connection-strings \
    --query "connectionStrings[0].connectionString" -o tsv)"

dotnet run
```

Åbn derefter URL'en, som vises i konsollen (fx `http://localhost:5xxx`).

Database- og containernavn står i `appsettings.json` (`CosmosDb:DatabaseName` og `CosmosDb:ContainerName`).

Kører appen på en VM eller i Azure, kan connection string sættes som miljøvariabel:

```bash
export CosmosDb__ConnectionString="AccountEndpoint=...;AccountKey=...;"
```

## Status

**Det har vi nået:**

- Blazor WebApp oprettet med `dotnet new blazor`
- Modelklasse med validering (påkrævede felter, email, telefon, længde)
- `CosmosService`, registreret med dependency injection, som kan indsætte og hente henvendelser
- Side til oprettelse af henvendelser med fejlbeskeder i UI'et
- Side med oversigt over alle henvendelser
- Navigation mellem siderne, ny forside, Counter- og Weather-siderne fjernet
- Connection string holdt ude af Git med user-secrets

**Det mangler:**

- Redigering og sletning af henvendelser
- Status på henvendelser (fx ny / i gang / løst) og svar fra medarbejdere
- Login, så kun medarbejdere kan se listen

**Næste trin:**

- Filtrér listen på kategori (udnytter partition key)
- Deploy til Azure App Service eller VM, med connection string i Azure Key Vault
- Paging på listen, når antallet af henvendelser vokser
