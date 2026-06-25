-- ============================================================
-- detect_state.sql
-- Script de diagnóstico: muestra el estado actual de la base
-- para determinar qué migraciones necesita el cliente.
-- Ejecutar ANTES de migrate.sh.
--
-- Uso:
--   mysql -h HOST -u USER -pPASS pm_timesheet_evm < detect_state.sql
--   (o pm_clockify_evm si es un cliente muy antiguo)
-- ============================================================

-- 1. Nombre de la base actual
SELECT DATABASE() AS base_actual;

-- 2. Estado de tablas clockify_* (indica base MUY antigua, pre-Feb 2026)
SELECT
  CASE
    WHEN COUNT(*) > 0 THEN CONCAT('⚠ REQUIERE mig 002 — encontradas ', COUNT(*), ' tabla(s) clockify_*')
    ELSE '✓ Sin tablas clockify_* (rename ya aplicado)'
  END AS estado_rename_clockify
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME LIKE 'clockify_%';

-- 3. Tablas del sistema .NET que podrían faltar
SELECT
  expected.tabla,
  CASE
    WHEN t.TABLE_NAME IS NOT NULL THEN '✓ Existe'
    ELSE '✗ FALTA — necesita migración'
  END AS estado
FROM (
  SELECT 'timesheet_clients'               AS tabla, '001 (base)' AS desde_mig UNION ALL
  SELECT 'timesheet_projects',                        '001 (base)'              UNION ALL
  SELECT 'timesheet_users',                           '001 (base)'              UNION ALL
  SELECT 'timesheet_time_entries',                    '001 (base)'              UNION ALL
  SELECT 'profiles',                                  '001 (base)'              UNION ALL
  SELECT 'users',                                     '001 (base)'              UNION ALL
  SELECT 'etc_records',                               '001 (base)'              UNION ALL
  SELECT 'etc_snapshots',                             '001 (base)'              UNION ALL
  SELECT 'change_requests',                           '001 (base)'              UNION ALL
  SELECT 'modules',                                   '003'                     UNION ALL
  SELECT 'actions',                                   '003'                     UNION ALL
  SELECT 'permissions',                               '003'                     UNION ALL
  SELECT 'profile_permissions',                       '003'                     UNION ALL
  SELECT 'change_audit_log',                          '004'                     UNION ALL
  SELECT 'project_intake_category_refs',              '005'                     UNION ALL
  SELECT 'project_intake_type_refs',                  '005'                     UNION ALL
  SELECT 'project_intake_status_refs',                '005'                     UNION ALL
  SELECT 'project_intake_records',                    '005'                     UNION ALL
  SELECT 'project_trackings',                         '006'                     UNION ALL
  SELECT 'project_tracking_updates',                  '006'                     UNION ALL
  SELECT 'schema_migrations',                         'runner'
) expected
LEFT JOIN information_schema.TABLES t
  ON t.TABLE_SCHEMA = DATABASE()
  AND t.TABLE_NAME  = expected.tabla
ORDER BY expected.tabla;

-- 4. Historial de migraciones Laravel (si existe la tabla migrations)
SELECT
  CASE
    WHEN COUNT(*) > 0 THEN CONCAT('Laravel — ', COUNT(*), ' migraciones registradas (última: ',
      (SELECT migration FROM `migrations` ORDER BY id DESC LIMIT 1), ')')
    ELSE 'Sin tabla migrations de Laravel'
  END AS laravel_migrations
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'migrations';

-- 5. Migraciones .NET ya aplicadas (si ya se corrió migrate.sh alguna vez)
SELECT
  CASE
    WHEN COUNT(*) > 0 THEN 'schema_migrations YA existe'
    ELSE 'schema_migrations aún no existe — primera vez corriendo migrate.sh'
  END AS estado_schema_migrations
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'schema_migrations';

-- Si schema_migrations existe, mostrar su contenido
SELECT migration_name, applied_at
FROM `schema_migrations`
ORDER BY id;
