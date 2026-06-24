# BDT EVM — Backend .NET

Backend del sistema de gestión de recursos y seguimiento de proyectos.  
Migración del sistema original Laravel a **ASP.NET Core 9.0**.

---

## Stack tecnológico

| Componente | Tecnología |
|---|---|
| Framework | ASP.NET Core 9.0 |
| Base de datos | MySQL 8.0 (Pomelo EF Core) |
| ORM | Entity Framework Core 9.0 |
| Auth | Tokens tipo Sanctum + BCrypt |
| Integración | Clockify API |
| Documentación API | Swagger UI (Swashbuckle 6.9) |
| Contenedores | Docker + Docker Compose |

---

## Prerrequisitos

### Sin Docker

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- MySQL 8.0 corriendo en el host
- Visual Studio 2022 / Rider / VS Code

### Con Docker

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows / Mac)
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
   dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Port=3306;Database=pm_timesheet_evm;User=tu_user;Password=tu_password;"
   dotnet user-secrets set "Clockify:ApiKey" "tu_api_key"
   dotnet user-secrets set "Clockify:WorkspaceId" "tu_workspace_id"
   dotnet user-secrets set "Clockify:UserId" "tu_user_id"
   ```

3. **Correr la aplicación**
   ```bash
   dotnet run
   # La app levanta en http://localhost:5000 (ver launchSettings.json)
   ```

---

### Opción B — Con Docker (recomendada)

El compose levanta **dos contenedores** en una red interna compartida:
- `db_clockify_mysql` — MySQL 8.0 con todas las tablas y datos mock ya cargados
- `bdt_dotnet` — backend .NET (espera a que MySQL esté healthy antes de iniciar)

1. **Clonar el repositorio**
   ```bash
   git clone <repo-url>
   cd bdt_evm_app
   ```

2. **Configurar variables de entorno**
   ```bash
   cp .env.example .env
   # Editá .env con tus valores reales (Clockify keys, etc.)
   ```

   El `.env` mínimo necesario:
   ```env
   DB_CONNECTION_STRING=Server=mysql;Port=3306;Database=pm_timesheet_evm;User=bdt_user;Password=bdt_user;
   CLOCKIFY_API_KEY=tu_api_key
   CLOCKIFY_WORKSPACE_ID=tu_workspace_id
   CLOCKIFY_USER_ID=tu_user_id
   ```

   > `Server=mysql` es el nombre del servicio MySQL dentro de la red Docker — no necesitás IP ni `host.docker.internal`.

3. **Levantar los contenedores**
   ```bash
   docker compose up --build -d
   ```

   En el primer arranque MySQL ejecuta automáticamente todos los scripts de `mysql/init/` en orden, creando las tablas y cargando los datos mock.

4. **Verificar que todo funciona**
   ```bash
   curl http://localhost:5000/api/health
   # { "status": "ok", "database": "ok" }
   ```

> El backend tarda unos segundos extra en iniciar porque espera a que MySQL pase su healthcheck antes de arrancar.

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
| Ver logs en tiempo real | `docker compose logs -f backend` |
| Ver estado de los contenedores | `docker compose ps` |
| Entrar al contenedor del backend | `docker compose exec backend sh` |

### Base de datos

| Acción | Comando |
|---|---|
| Ver logs de MySQL | `docker compose logs -f mysql` |
| Entrar a la consola MySQL | `docker compose exec mysql mysql -u bdt_user -pbdt_user pm_timesheet_evm` |
| Resetear la DB (borra y re-crea con el schema) | `docker compose down -v && docker compose up -d` |

> `docker compose down -v` elimina el volumen de datos. Usarlo solo cuando querés empezar desde cero con el schema limpio.

---

## Base de datos

### Init — entorno de desarrollo

La carpeta `mysql/init/` contiene los scripts que MySQL ejecuta automáticamente al crear el volumen por primera vez (orden alfabético):

| Archivo | Contenido |
|---|---|
| `01_schema.sql` | Todas las tablas del sistema + datos mock base |
| `02_schema.sql` | Tablas RBAC (modules, actions, permissions, profile_permissions) |
| `03_schema.sql` | Tabla de auditoría (change_audit_log) |
| `04_schema.sql` | Seed del RBAC (módulos, permisos y asignaciones por perfil) |
| `05_seed_evm.sql` | Proyectos mock con datos EVM para el dashboard |
| `06_seed_change_requests.sql` | Change requests asociados a los proyectos mock |

Si el volumen ya existe (arranques posteriores), MySQL **no** vuelve a ejecutar los scripts — los datos persisten entre reinicios.

Para resetear la base a su estado inicial:

```bash
docker compose down -v && docker compose up -d
```

### Migraciones — clientes con datos preexistentes

La carpeta `mysql/migrations/` contiene el sistema de migraciones para aplicar cambios estructurales sobre una base que ya tiene datos, donde no es posible recrear el volumen.

Los scripts de `migrations/` **nunca se ejecutan solos** — no están montados en el contenedor. Se aplican manualmente corriendo `mysql/migrate.sh` con las credenciales de la BD del cliente.

#### Archivos de migración

| Archivo | Tipo | Qué hace |
|---|---|---|
| `001_create_schema_migrations.sql` | DDL | Crea la tabla de tracking `schema_migrations` |
| `002_rename_clockify_to_timesheet.sql` | DDL | Renombra tablas/columnas `clockify_*` → `timesheet_*` (solo si aplica) |
| `003_add_rbac_tables.sql` | DDL | Crea las tablas RBAC |
| `004_add_audit_log.sql` | DDL | Crea `change_audit_log` |
| `005_add_intake_tables.sql` | DDL | Crea las tablas de Intake de proyectos |
| `005b_seed_intake_refs.sql` | Seed | Carga las tablas de referencia del Intake (categorías, tipos, estados) |
| `006_add_project_tracking_tables.sql` | DDL | Crea `project_trackings` y `project_tracking_updates` |
| `007_seed_rbac_data.sql` | Seed | Carga módulos, acciones, permisos y asignaciones por perfil |

Todas las migraciones son **idempotentes**: se pueden correr varias veces sin efecto duplicado.

#### Uso

1. **Diagnóstico** — verificar el estado actual de la base del cliente:

   ```bash
   mysql -h HOST -u bdt_user -pbdt_user pm_timesheet_evm < mysql/detect_state.sql
   ```

   Muestra qué tablas existen, cuáles faltan y qué migraciones ya se aplicaron.

2. **Migrar** — aplicar los cambios pendientes:

   ```bash
   chmod +x mysql/migrate.sh
   ./mysql/migrate.sh -h HOST -u bdt_user -p bdt_user
   ```

   O usando variables de entorno:

   ```bash
   DB_HOST=HOST DB_PASSWORD=xxx ./mysql/migrate.sh
   ```

   El runner aplica solo las migraciones pendientes en orden, registra cada una en `schema_migrations` y se detiene ante el primer error. Al volver a correr, saltea las que ya están aplicadas.

#### Diferencia entre init y migrations

| | `mysql/init/` | `mysql/migrations/` |
|---|---|---|
| Cuándo corre | Automático, primer boot del volumen | Manual, con `migrate.sh` |
| Sobre qué base | Vacía / nueva | Existente con datos |
| A quién sirve | Equipo de desarrollo | Clientes en producción |

---

## Agregar una nueva variable de entorno

1. **Documentar en `.env.example`** *(va al repo)*
   ```env
   MI_NUEVA_VAR=
   ```

2. **Agregar valor en `.env`** *(no va al repo)*
   ```env
   MI_NUEVA_VAR=valor_real
   ```

3. **Inyectar en `docker-compose.yml`**
   ```yaml
   environment:
     Mi__NuevaVar: ${MI_NUEVA_VAR}
   ```

   > En .NET el separador `__` mapea a jerarquía de configuración: `Mi__NuevaVar` → `Mi:NuevaVar`

4. **Agregar en User Secrets** *(para desarrollo sin Docker)*
   ```bash
   dotnet user-secrets set "Mi:NuevaVar" "valor_real"
   ```

5. **Reiniciar el contenedor**
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

1. Llamar a `POST /api/auth/login` o `POST /api/auth/login-with-profile` (no requieren token)
2. Copiar el valor del campo `token` de la respuesta
3. Hacer clic en el botón **Authorize** (arriba a la derecha en Swagger)
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
