# 🎬 MovieApi

An ASP.NET Core 10 Web API for managing a movie database — genres, actors, movies, reviews, and cast relationships. Built with Entity Framework Core, SQL Server, JWT authentication, and Docker support.

## Quick Start (Docker)

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- get docker-compose.yml file from me
- A `.env` file at `/.env` with these keys:
  ```
  ASPNETCORE_ENVIRONMENT=Development
  DOCKER_SQL_PASSWORD=<your-strong-password>
  VERCEL_BLOB_TOKEN=<your-token>
  JWT_SECRET=<your-jwt-secret>
  DOCKER_SQL_CONNECTIONSTRING=<your-connectionstring>
  ```
- "docker compose up -d" in terminal

### Option A — Build from source (with .NET SDK)
```bash
docker compose up
```
Builds the Dockerfile, starts SQL Server + the API.

### Option B — Pre-built image (no SDK needed)
1. Open `docker-compose.consumer.yml` and replace `yourusername` with the actual Docker Hub username.
2. Run:
```bash
docker compose -f docker-compose.consumer.yml up
```

### Access
- API: **http://localhost:8080**
- OpenAPI docs (dev mode): **http://localhost:8080/openapi**

