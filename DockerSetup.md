## System Requirements

Before running AuthWallet, ensure your development environment meets the following requirements.

### Hardware

| Component | Minimum | Recommended |
|---|---|---|
| CPU | 2 cores | 4+ cores |
| RAM | 8 GB | 16 GB |
| Storage | 20 GB free | 30+ GB free |
| Network | Internet connection | Stable broadband connection |

### Operating System

- Windows 10 64-bit or later
- Windows 11 64-bit
- Linux distributions supported by Docker Desktop / Docker Engine
- macOS supported by Docker Desktop

### Required Software

The application is containerized using Docker, so the following software is required:

- **Docker Desktop** with Linux container support
- **Docker Compose v2**
- **Git** for cloning the repository

The following runtimes are **not required to be installed directly on the host machine** when running the application through Docker:

- .NET SDK
- Node.js / npm
- SQL Server

These components are provided through Docker containers.

### Docker Services

AuthWallet consists of the following services:

| Service | Technology | Default Port |
|---|---|---:|
| Frontend | React + Vite + Nginx | `3000` |
| Backend | ASP.NET Core / .NET | `8080` |
| Database | Microsoft SQL Server 2022 | `1433` |

Make sure these ports are available on the host machine, or update the port mappings in `docker-compose.yml`.

### Environment Configuration

Create a `.env` file in the project root directory before starting the application.

Example:

```env
MSSQL_SA_PASSWORD=YourStrongPassword123!
```

> **Security:** Do not commit the `.env` file to source control. Add it to `.gitignore`.

### Running with Docker

From the project root directory, run:

```bash
docker compose up --build
```

To run the services in detached mode:

```bash
docker compose up -d --build
```

To stop the application:

```bash
docker compose down
```

The application will be available at:

```text
Frontend: http://localhost:3000
Backend:  http://localhost:8080
SQL Server: localhost:1433
```

### Docker Resource Recommendation

For a smoother development experience, especially when running SQL Server together with the frontend and backend containers, **16 GB of system RAM is recommended**.