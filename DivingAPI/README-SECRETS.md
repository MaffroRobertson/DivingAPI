Local development (dotnet user-secrets)

1. Initialize user-secrets for the project (run in project directory):

   dotnet user-secrets init

2. Add the JWT key to user-secrets (example):

   dotnet user-secrets set "Jwt:Key" "<your-long-random-secret>"

3. Add DB connection if needed:

   dotnet user-secrets set "ConnectionStrings:LocalDB" "Server=(localdb)\\mssqllocaldb;Database=divingDB;Trusted_Connection=True;"

Docker / docker-compose (development)

- Do not hard-code secrets in `docker-compose.yml`. Use environment variables passed at runtime or a `.env` file not checked into source.

Example using environment variables (compose file):

  api:
    environment:
      - Jwt__Key=${JWT_KEY}
      - ConnectionStrings__LocalDB=${LOCAL_DB_CONN}

Then set them locally before running compose:

  # powershell
  $env:JWT_KEY = "<your-secret>"
  $env:LOCAL_DB_CONN = "Server=db;Database=DivingDb;User Id=sa;Password=...;"

Production

- Use a secrets manager (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault) and wire it into `Configuration`.
- Rotate keys and follow the principle of least privilege.
