## Manual Setup and Running

If you prefer to run AuthWallet without Docker, you need to install the required runtime and database directly on your machine.

### Prerequisites

Install the following software:

- **.NET 8 SDK**
- **Node.js 20+**
- **npm**
- **Microsoft SQL Server 2022**
- **Git**

Verify the installations:

```bash
dotnet --version
node --version
npm --version
```

### 1. Configure SQL Server

Make sure SQL Server is running.

Create a database named:

```text
AuthWalletDb
```

For example, using SQL Server Management Studio (SSMS):

```sql
CREATE DATABASE AuthWalletDb;
```

Update the connection string in the backend configuration.

For example, in:

```text
AuthWallet.Api/appsettings.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AuthWalletDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True"
  }
}
```

Adjust the `Server`, `User Id`, and `Password` according to your SQL Server configuration.

### 2. Apply Entity Framework Core Migrations

From the solution root directory:

```bash
dotnet ef database update \
  --project AuthWallet.Infrastructure \
  --startup-project AuthWallet.Api
```

On Windows PowerShell, the same command can be written as:

```powershell
dotnet ef database update --project AuthWallet.Infrastructure --startup-project AuthWallet.Api
```

If `dotnet ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

Verify:

```bash
dotnet ef --version
```

### 3. Run the Backend

From the solution root:

```bash
dotnet run --project AuthWallet.Api
```

Alternatively:

```bash
cd AuthWallet.Api
dotnet run
```

The API will start using the configured ASP.NET Core URLs.

For example:

```text
https://localhost:7215
```

You can verify the API through Swagger:

```text
https://localhost:7215/swagger
```

> The actual port may differ depending on your `launchSettings.json`.

### 4. Configure the Frontend

Go to the frontend directory:

```bash
cd AuthWallet.frontend
```

Install dependencies:

```bash
npm install
```

Start the Vite development server:

```bash
npm run dev
```

The frontend will normally be available at:

```text
http://localhost:3000
```

### 5. Configure API Proxy

For manual development, Vite can proxy API requests to the backend.

Example `vite.config.js`:

```javascript
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      "/api": {
        target: "https://localhost:7215",
        changeOrigin: true,
        secure: false
      }
    }
  }
});
```

The frontend should call the API using relative URLs:

```javascript
client.get("/api/auth/me");
```

rather than:

```javascript
client.get("https://localhost:7215/api/auth/me");
```

This allows the same frontend code to work with the Vite proxy during development and the Nginx proxy when running with Docker.

### 6. Running the Complete Application Manually

You need three components running:

**Terminal 1 — SQL Server**

Make sure the SQL Server service is running.

**Terminal 2 — Backend**

```bash
dotnet run --project AuthWallet.Api
```

**Terminal 3 — Frontend**

```bash
cd AuthWallet.frontend
npm run dev
```

Then open:

```text
http://localhost:3000
```

### Quick Start — Manual

After everything has been configured:

```powershell
# Terminal 1
dotnet run --project AuthWallet.Api

# Terminal 2
cd AuthWallet.frontend
npm run dev
```

Make sure SQL Server is running before starting the backend.

Then open:

```text
http://localhost:3000
```
