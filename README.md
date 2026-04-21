# BDT EVM — Backend .NET

Backend del sistema de gestión de recursos y seguimiento de proyectos.  
Migración del sistema original Laravel a **ASP.NET Core 9.0**.

---

## Stack tecnológico

| Componente | Tecnología |
|---|---|
| Framework | ASP.NET Core 9.0 |
| Base de datos | MySQL (Pomelo EF Core) |
| ORM | Entity Framework Core 9.0 |
| Auth | Tokens tipo Sanctum + BCrypt |
| Integración | Clockify API |
| Contenedores | Docker + Docker Compose |

---

## Prerrequisitos

### Sin Docker (desarrollo local)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- MySQL 8.0 corriendo en el host
- Visual Studio 2022 / Rider / VS Code

### Con Docker
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows o Mac)
- MySQL 8.0 corriendo en el host *(Fase 1 — desarrollo en paralelo con Laravel)*

---

## Instalación

### Opción A — Sin Docker (`dotnet run`)

1. **Clonar el repositorio**
   ```bash
   git clone <repo-url>
   cd bdt_evm_app
   ```

2. **Configurar User Secrets** (nunca se commitean)
   ```bash
   dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Port=3306;Database=pm_clockify_evm;User=tu_user;Password=tu_password;"
   dotnet user-secrets set "Clockify:ApiKey" "tu_api_key"
   dotnet user-secrets set "Clockify:WorkspaceId" "tu_workspace_id"
   dotnet user-secrets set "Clockify:UserId" "tu_user_id"
   ```

3. **Correr la aplicación**
   ```bash
   dotnet run
   # La app levanta en http://localhost:5000 (o el puerto configurado ver launchSettings.json)
   ```

---

### Opción B — Con Docker

1. **Clonar el repositorio**
   ```bash
   git clone <repo-url>
   cd bdt_evm_app
   ```

2. **Configurar variables de entorno**
   ```bash
   cp .env.example .env
   # Editá .env con tus valores reales
   ```

   El `.env` mínimo necesario:
   ```env
   DB_CONNECTION_STRING=Server=host.docker.internal;Port=3306;Database=pm_clockify_evm;User=tu_user;Password=tu_password;
   CLOCKIFY_API_KEY=tu_api_key
   CLOCKIFY_WORKSPACE_ID=tu_workspace_id
   CLOCKIFY_USER_ID=tu_user_id
   ```

3. **Permitir conexiones desde Docker al MySQL del host**

   Ejecutar en MySQL (una sola vez por colega):
   ```sql
   CREATE USER IF NOT EXISTS 'tu_user'@'%' IDENTIFIED BY 'tu_password';
   GRANT ALL PRIVILEGES ON pm_clockify_evm.* TO 'bdt_user'@'%';
   FLUSH PRIVILEGES;
   ```
   > Esto es necesario porque Docker se conecta al MySQL del host desde la IP `172.18.x.x`, no desde `localhost`.

4. **Levantar el contenedor**
   ```bash
   docker compose up --build -d
   ```

5. **Verificar que todo funciona**
   ```bash
   curl http://localhost:5000/api/health
   # Respuesta esperada: { "status": "ok", "database": "ok" }
   ```

---

## Variables de entorno

| Variable | Descripción | Requerida |
|---|---|---|
| `DB_CONNECTION_STRING` | Connection string completa de MySQL | ✅ |
| `CLOCKIFY_API_KEY` | API Key de Clockify | ✅ |
| `CLOCKIFY_WORKSPACE_ID` | ID del workspace en Clockify | ✅ |
| `CLOCKIFY_USER_ID` | ID del usuario en Clockify | ✅ |
| `ASPNETCORE_ENVIRONMENT` | `Development` o `Production` | — |
| `BACKEND_PORT` | Puerto expuesto (default: `5000`) | — |

---

## Comandos Docker de referencia

| Acción | Comando |
|---|---|
| Levantar (primera vez o con cambios de código) | `docker compose up --build -d` |
| Levantar sin reconstruir | `docker compose up -d` |
| Bajar el contenedor | `docker compose down` |
| Reiniciar (solo cambios de variables) | `docker compose down && docker compose up -d` |
| Ver logs en tiempo real | `docker compose logs -f backend` |
| Ver estado del contenedor | `docker compose ps` |
| Entrar al contenedor | `docker compose exec backend sh` |

---

## Agregar una nueva variable de entorno

Cuando agregás una nueva configuración al proyecto, seguir este flujo:

**1. Documentar en `.env.example`** *(va al repo)*
```env
MI_NUEVA_VAR=
```

**2. Agregar valor en `.env`** *(no va al repo)*
```env
MI_NUEVA_VAR=valor_real
```

**3. Inyectar en `docker-compose.yml`**
```yaml
environment:
  Mi__NuevaVar: ${MI_NUEVA_VAR}
```
> En .NET el separador `__` mapea a jerarquía de configuración: `Mi__NuevaVar` → `Mi:NuevaVar`

**4. Agregar en User Secrets** *(para desarrollo sin Docker)*
```bash
dotnet user-secrets set "Mi:NuevaVar" "valor_real"
```

**5. Reiniciar el contenedor**
```bash
docker compose down && docker compose up -d
```

---

## Estrategia de despliegue

Este backend convive en paralelo con el sistema original Laravel durante el desarrollo.

```
FASE 1 — Desarrollo en paralelo (actual)
├── Docker: solo backend .NET
├── MySQL: host local compartido con Laravel (proyecto original)
└── Frontend: apunta a Laravel

FASE 2 — Testing
├── Docker: backend .NET
├── MySQL: host local
└── Frontend: apunta al .NET

FASE 3 — Switch (go live)
├── docker-compose completo: .NET + MySQL
├── Se apaga Laravel
└── Frontend apunta al nuevo backend
```

---

## Health check

```
GET /api/health
```

Verifica el estado de la aplicación y la conexión a la base de datos.

```json
{
  "status": "ok",
  "timestamp": "2025-05-01T12:00:00Z",
  "database": "ok"
}
```

| `database` | Significado |
|---|---|
| `ok` | MySQL conectado correctamente |
| `error` | Error de conexión a MySQL |

---
