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
| Documentación API | Swagger UI (Swashbuckle 6.9) |
| Contenedores | Docker + Docker Compose |

---

## Prerrequisitos

### Sin Docker (desarrollo local)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- MySQL 8.0 corriendo en el host
- Visual Studio 2022 / Rider / VS Code

### Con Docker
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows o Mac)
- Nada más — MySQL corre como contenedor junto con el backend

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

### Opción B — Con Docker (recomendada)

El compose levanta **dos contenedores** en una red interna compartida:
- `db_clockify_mysql` — MySQL 8.0 con las tablas y datos mock ya cargados
- `bdt_dotnet` — el backend .NET (espera a que MySQL esté healthy antes de iniciar)

1. **Clonar el repositorio**
   ```bash
   git clone <repo-url>
   cd bdt_evm_app
   ```

2. **Configurar variables de entorno**
   ```bash
   cp .env.example .env
   # Editá .env con tus valores reales (Clockify keys)
   ```

   El `.env` mínimo necesario:
   ```env
   DB_CONNECTION_STRING=Server=mysql;Port=3306;Database=pm_clockify_evm;User=bdt_user;Password=bdt_user;
   CLOCKIFY_API_KEY=tu_api_key
   CLOCKIFY_WORKSPACE_ID=tu_workspace_id
   CLOCKIFY_USER_ID=tu_user_id
   ```

   > `Server=mysql` es el nombre del servicio MySQL dentro de la red Docker — no necesitás IP ni `host.docker.internal`.

3. **Levantar los contenedores**
   ```bash
   docker compose up --build -d
   ```

   En el primer arranque, MySQL ejecuta automáticamente `mysql/init/01_schema.sql`
   que crea todas las tablas y carga los datos mock de base.

4. **Verificar que todo funciona**
   ```bash
   curl http://localhost:5000/api/health
   # Respuesta esperada: { "status": "ok", "database": "ok" }
   ```

> **Nota:** el backend tarda unos segundos extra en iniciar porque espera a que MySQL pase su healthcheck antes de arrancar.

---

## Variables de entorno

| Variable | Descripción | Requerida |
|---|---|---|
| `DB_CONNECTION_STRING` | Connection string de MySQL (usar `Server=mysql` con Docker) | ✅ |
| `CLOCKIFY_API_KEY` | API Key de Clockify | ✅ |
| `CLOCKIFY_WORKSPACE_ID` | ID del workspace en Clockify | ✅ |
| `CLOCKIFY_USER_ID` | ID del usuario en Clockify | ✅ |
| `ASPNETCORE_ENVIRONMENT` | `Development` o `Production` | — |
| `BACKEND_PORT` | Puerto expuesto en el host (default: `7100`) | — |

---

## Comandos Docker de referencia

### Backend

| Acción | Comando |
|---|---|
| Levantar (primera vez o con cambios de código) | `docker compose up --build -d` |
| Levantar sin reconstruir | `docker compose up -d` |
| Bajar los contenedores | `docker compose down` |
| Reiniciar (solo cambios de variables) | `docker compose down && docker compose up -d` |
| Ver logs del backend en tiempo real | `docker compose logs -f backend` |
| Ver estado de los contenedores | `docker compose ps` |
| Entrar al contenedor del backend | `docker compose exec backend sh` |

### Base de datos

| Acción | Comando |
|---|---|
| Ver logs de MySQL | `docker compose logs -f mysql` |
| Entrar a la consola MySQL | `docker compose exec mysql mysql -u bdt_user -pbdt_user pm_clockify_evm` |
| Resetear la DB (borra y re-crea con el schema) | `docker compose down -v && docker compose up -d` |

> ⚠️ `docker compose down -v` elimina el volumen de datos. Usarlo solo cuando querés empezar desde cero con el schema limpio.

---

## Base de datos — schema e init

El archivo `mysql/init/01_schema.sql` se ejecuta automáticamente la primera vez que se levanta el contenedor de MySQL (cuando el volumen `mysql_data` no existe).

Incluye:
- Creación de las 32 tablas del sistema
- Datos mock de base (usuarios, clientes, configuraciones iniciales)

Si el volumen ya existe (arranques posteriores), MySQL **no** vuelve a ejecutar el script — los datos persisten entre reinicios.

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

## Documentación de la API (Swagger)

Con la app corriendo, la documentación interactiva está disponible en:

```
http://localhost:5000/swagger
```

Desde ahí podés explorar todos los endpoints, ver los esquemas de request/response y ejecutar llamadas directamente desde el browser.

### Autenticación en Swagger

Los endpoints protegidos requieren un token Bearer. Para autenticarte:

1. Llamar a `POST /api/auth/login` o `POST /api/auth/login-with-profile` desde Swagger (no requieren token)
2. Copiar el valor del campo `token` de la respuesta
3. Hacer clic en el botón **Authorize** (arriba a la derecha)
4. Ingresar el token con el formato: `Bearer <token>`
5. Confirmar con **Authorize** — a partir de ese momento todas las llamadas incluyen el header automáticamente

> La ruta `/swagger` no requiere autenticación — está excluida del middleware de auth.

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
