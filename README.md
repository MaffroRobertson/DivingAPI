# DivingAPI

Simple REST API for logging dives.

## Overview

DivingAPI is a small REST API intended to record and manage dive logs. The project is primarily written in **C#** and includes **Docker** support.

## Features

- REST endpoints for working with dive log data (create/read/update/delete)
- JSON over HTTP
- Container-friendly (Dockerfile included)

> Note: The exact routes, request/response shapes, and authentication depend on the current implementation in the repo.

## Tech Stack

- **C# / .NET** (primary)
- **Docker** (containerization)

## Getting Started

### Prerequisites

- **.NET SDK** (version depends on the project; see `global.json` or `.csproj` files if present)
- Optional: **Docker**

### Run locally (dotnet)

1. Clone the repo:
   ```bash
   git clone https://github.com/MaffroRobertson/DivingAPI.git
   cd DivingAPI
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the API:
   ```bash
   dotnet run
   ```

4. If Swagger/OpenAPI is enabled, check the console output for the URL and open it in a browser.

### Run with Docker

1. Build the image:
   ```bash
   docker build -t divingapi .
   ```

2. Run the container:
   ```bash
   docker run --rm -p 8080:8080 divingapi
   ```

3. Open the API in your browser / API client:
   - `http://localhost:8080`

> If your app listens on a different port internally (common: 5000/5001 or 8080), update the `-p` mapping accordingly.

## Configuration

Configuration is typically provided via `appsettings.json` and/or environment variables.

Common examples (names may differ depending on the codebase):

- `ASPNETCORE_ENVIRONMENT` (e.g., `Development`, `Staging`, `Production`)
- `ASPNETCORE_URLS` (e.g., `http://+:8080`)
- Connection strings (if a database is used)

Example:
```bash
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:8080
dotnet run
```

## API Documentation

If the project has Swagger enabled, you can usually find it at one of:

- `/swagger`
- `/swagger/index.html`

Example:
- `http://localhost:8080/swagger`

## Testing

If tests exist in the repository:

```bash
dotnet test
```

## License

Add a license file to the repository (recommended: `LICENSE`) and update this section accordingly.
