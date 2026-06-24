#!/usr/bin/env bash
# ============================================================
# migrate.sh — Runner de migraciones para clientes con datos
# preexistentes. Aplica solo las migraciones faltantes, en orden.
#
# Uso:
#   ./migrate.sh                           # usa valores por defecto / .env
#   ./migrate.sh -h 192.168.1.10 -P 3306 -u bdt_user -p bdt_user -d pm_timesheet_evm
#
# Variables de entorno (alternativa a flags):
#   DB_HOST, DB_PORT, DB_USER, DB_PASSWORD, DB_DATABASE
#
# Vía Docker (si la base corre en el contenedor local):
#   docker exec -i db_clockify_mysql bash -c \
#     "mysql -u bdt_user -pbdt_user pm_timesheet_evm" < migrations/001_create_schema_migrations.sql
#   -- o correr este script desde el host apuntando a 127.0.0.1:3306
# ============================================================

set -euo pipefail

# ── Parámetros de conexión ────────────────────────────────────
DB_HOST="${DB_HOST:-127.0.0.1}"
DB_PORT="${DB_PORT:-3306}"
DB_USER="${DB_USER:-bdt_user}"
DB_PASSWORD="${DB_PASSWORD:-bdt_user}"
DB_DATABASE="${DB_DATABASE:-pm_timesheet_evm}"

while getopts "h:P:u:p:d:" opt; do
  case $opt in
    h) DB_HOST="$OPTARG"     ;;
    P) DB_PORT="$OPTARG"     ;;
    u) DB_USER="$OPTARG"     ;;
    p) DB_PASSWORD="$OPTARG" ;;
    d) DB_DATABASE="$OPTARG" ;;
    *) echo "Uso: $0 [-h host] [-P port] [-u user] [-p password] [-d database]"; exit 1 ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MIGRATIONS_DIR="${SCRIPT_DIR}/migrations"

# Exportar password como variable de entorno para evitar warning de CLI
export MYSQL_PWD="${DB_PASSWORD}"
MYSQL_CMD="mysql -h${DB_HOST} -P${DB_PORT} -u${DB_USER} ${DB_DATABASE}"

# ── Verificar conexión ────────────────────────────────────────
echo "Conectando a ${DB_USER}@${DB_HOST}:${DB_PORT}/${DB_DATABASE} ..."
if ! ${MYSQL_CMD} -e "SELECT 1;" > /dev/null 2>&1; then
  echo "ERROR: No se pudo conectar a la base de datos."
  echo "Verificá las credenciales y que MySQL esté accesible."
  exit 1
fi
echo "Conexión OK."
echo ""

# ── Crear tabla de tracking si no existe ─────────────────────
${MYSQL_CMD} <<'SQL'
CREATE TABLE IF NOT EXISTS `schema_migrations` (
  `id`             INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `migration_name` VARCHAR(255) NOT NULL,
  `applied_at`     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_schema_migrations_name` (`migration_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
SQL

# ── Aplicar migraciones pendientes ───────────────────────────
applied=0
skipped=0
failed=0

for file in "${MIGRATIONS_DIR}"/[0-9]*.sql; do
  [ -f "$file" ] || continue
  name="$(basename "$file" .sql)"

  already=$(${MYSQL_CMD} --skip-column-names --silent \
    -e "SELECT COUNT(*) FROM schema_migrations WHERE migration_name = '${name}';")

  if [ "${already}" -gt 0 ]; then
    echo "  ✓ Ya aplicada: ${name}"
    ((skipped++)) || true
    continue
  fi

  echo "→ Aplicando: ${name} ..."
  if ${MYSQL_CMD} < "${file}"; then
    ${MYSQL_CMD} -e \
      "INSERT IGNORE INTO schema_migrations (migration_name) VALUES ('${name}');"
    echo "  ✓ OK"
    ((applied++)) || true
  else
    echo "  ✗ FALLÓ: ${name}"
    echo ""
    echo "La migración falló. Revisá el error arriba antes de continuar."
    echo "Una vez corregido, podés volver a correr migrate.sh — saltará las ya aplicadas."
    ((failed++)) || true
    exit 1
  fi
done

# ── Resumen ───────────────────────────────────────────────────
echo ""
echo "============================================"
echo "  Migraciones aplicadas:  ${applied}"
echo "  Ya estaban al día:      ${skipped}"
if [ "${failed}" -gt 0 ]; then
  echo "  Fallidas:               ${failed}"
fi
echo "============================================"

if [ "${applied}" -eq 0 ] && [ "${failed}" -eq 0 ]; then
  echo "La base ya estaba actualizada. No hubo cambios."
else
  echo "Migraciones completadas exitosamente."
fi
