-- =====================================================================
-- 05_seed_evm.sql
-- Seed adicional para el Dashboard EVM (RF-10).
--
-- Carga 8 proyectos nuevos con datos completos:
--   - timesheet_projects con BAC base/total > 0 y hourly_rate definido
--   - timesheet_time_entries para que AC sea > 0 (suma reportada por Clockify)
--   - etc_snapshots + etc_records para que ETC sea > 0 (snapshot vigente)
--   - project_trackings + project_tracking_updates para "Control de cambios"
--
-- Cubre todos los casos del RF-10 para la columna "Control de cambios":
--   - Proyectos sin tracking en DB                       -> "No"
--   - Proyectos con tracking pero sin updates            -> "No"
--   - Proyectos con N>0 updates                          -> "Sí (N)"  (N=1,2,3,4)
--
-- IDs reservados:
--   timesheet_projects:        11..18
--   timesheet_time_entries:    8..40
--   etc_snapshots:            5..12
--   etc_records:              14..40
--   project_trackings:        3..9
--   project_tracking_updates: 3..15
--
-- Clientes usados (existentes en 01_schema.sql):
--   3 = BDT Global
--   4 = Cliente Test 1
--   5 = Google Inc
--   6 = Anthropic Testing Corp
-- =====================================================================

USE pm_timesheet_evm;

-- ---------------------------------------------------------------------
-- timesheet_projects
-- ---------------------------------------------------------------------
INSERT INTO `timesheet_projects`
  (`id`, `timesheet_project_id`, `name`, `code`, `client_id`, `status`,
   `start_date`, `end_date_planned`, `end_date_actual`,
   `bac_base_hours`, `bac_base_cost`, `bac_total_hours`, `bac_total_cost`,
   `hourly_rate`, `etc_calculation_mode`, `created_at`, `updated_at`)
VALUES
  (11, 'seed_evm_p11', 'Sistema Cotizador Web',          '30.110', 4, 'activo',
       '2026-01-15', '2026-07-30', NULL,
       800.00,  48000.00,  850.00,  51000.00, 60.00, 'manual',    '2026-01-15 10:00:00', '2026-04-20 10:00:00'),

  (12, 'seed_evm_p12', 'Migración Core BDT',             '30.111', 3, 'activo',
       '2025-11-01', '2026-12-31', NULL,
       1500.00, 90000.00, 1620.00, 97200.00, 60.00, 'manual',    '2025-11-01 10:00:00', '2026-04-21 10:00:00'),

  (13, 'seed_evm_p13', 'Integración Gemini API',         '30.112', 5, 'activo',
       '2026-02-10', '2026-06-15', NULL,
       400.00,  24000.00,  420.00,  25200.00, 60.00, 'automatic', '2026-02-10 10:00:00', '2026-04-18 10:00:00'),

  (14, 'seed_evm_p14', 'Auditoría Compliance Anthropic', '30.113', 6, 'activo',
       '2026-01-05', '2026-08-20', NULL,
       600.00,  36000.00,  600.00,  36000.00, 60.00, 'manual',    '2026-01-05 10:00:00', '2026-04-15 10:00:00'),

  (15, 'seed_evm_p15', 'Reporte EVM Mensual',            '30.114', 3, 'activo',
       '2026-03-01', '2026-09-30', NULL,
       300.00,  18000.00,  300.00,  18000.00, 60.00, 'manual',    '2026-03-01 10:00:00', '2026-04-10 10:00:00'),

  (16, 'seed_evm_p16', 'Onboarding Digital v2',          '30.115', 4, 'pausado',
       '2025-12-01', '2026-10-31', NULL,
       950.00,  57000.00, 1050.00, 63000.00, 60.00, 'automatic', '2025-12-01 10:00:00', '2026-04-25 10:00:00'),

  (17, 'seed_evm_p17', 'Dashboard Analytics Google',     '30.116', 5, 'cerrado',
       '2025-08-01', '2026-03-31', '2026-04-05',
       700.00,  42000.00,  740.00,  44400.00, 60.00, 'manual',    '2025-08-01 10:00:00', '2026-04-05 18:00:00'),

  (18, 'seed_evm_p18', 'Plataforma Soporte Anthropic',   '30.117', 6, 'activo',
       '2026-02-20', '2026-11-30', NULL,
       250.00,  15000.00,  250.00,  15000.00, 60.00, 'manual',    '2026-02-20 10:00:00', '2026-04-12 10:00:00');

-- ---------------------------------------------------------------------
-- timesheet_time_entries (drive AC real)
-- Distribución pensada para que cada proyecto tenga horas razonables vs BAC.
-- user_id existentes: 4,5,6,7,8,15
-- ---------------------------------------------------------------------
INSERT INTO `timesheet_time_entries`
  (`id`, `timesheet_time_entry_id`, `project_id`, `user_id`, `description`,
   `start_time`, `end_time`, `duration_hours`, `billable`,
   `change_request_id`, `source_raw`, `created_at`, `updated_at`)
VALUES
  -- Proyecto 11 (BAC 850, AC objetivo ~500)
  (8,  'te_seed_p11_a', 11, 5, 'Análisis funcional',          '2026-02-01 09:00:00', '2026-02-01 18:00:00', 160.000, 1, NULL, NULL, '2026-02-01 18:00:00', '2026-02-01 18:00:00'),
  (9,  'te_seed_p11_b', 11, 6, 'Desarrollo backend cotizador','2026-02-15 09:00:00', '2026-02-15 18:00:00', 180.000, 1, NULL, NULL, '2026-02-15 18:00:00', '2026-02-15 18:00:00'),
  (10, 'te_seed_p11_c', 11, 8, 'Desarrollo front',            '2026-03-10 09:00:00', '2026-03-10 18:00:00', 160.000, 1, NULL, NULL, '2026-03-10 18:00:00', '2026-03-10 18:00:00'),

  -- Proyecto 12 (BAC 1620, AC objetivo ~800)
  (11, 'te_seed_p12_a', 12, 5, 'Diseño arquitectura',         '2025-12-01 09:00:00', '2025-12-01 18:00:00', 260.000, 1, NULL, NULL, '2025-12-01 18:00:00', '2025-12-01 18:00:00'),
  (12, 'te_seed_p12_b', 12, 6, 'Implementación módulo A',     '2026-01-15 09:00:00', '2026-01-15 18:00:00', 270.000, 1, NULL, NULL, '2026-01-15 18:00:00', '2026-01-15 18:00:00'),
  (13, 'te_seed_p12_c', 12, 7, 'Implementación módulo B',     '2026-03-01 09:00:00', '2026-03-01 18:00:00', 270.000, 1, NULL, NULL, '2026-03-01 18:00:00', '2026-03-01 18:00:00'),

  -- Proyecto 13 (BAC 420, AC objetivo ~250)
  (14, 'te_seed_p13_a', 13, 8, 'Setup Gemini SDK',            '2026-02-15 09:00:00', '2026-02-15 18:00:00', 120.000, 1, NULL, NULL, '2026-02-15 18:00:00', '2026-02-15 18:00:00'),
  (15, 'te_seed_p13_b', 13, 6, 'Pruebas integración',         '2026-03-20 09:00:00', '2026-03-20 18:00:00', 130.000, 1, NULL, NULL, '2026-03-20 18:00:00', '2026-03-20 18:00:00'),

  -- Proyecto 14 (BAC 600, AC objetivo ~300)
  (16, 'te_seed_p14_a', 14, 5, 'Relevamiento controles',      '2026-01-20 09:00:00', '2026-01-20 18:00:00', 150.000, 1, NULL, NULL, '2026-01-20 18:00:00', '2026-01-20 18:00:00'),
  (17, 'te_seed_p14_b', 14, 7, 'Documentación auditoría',     '2026-02-25 09:00:00', '2026-02-25 18:00:00', 150.000, 1, NULL, NULL, '2026-02-25 18:00:00', '2026-02-25 18:00:00'),

  -- Proyecto 15 (BAC 300, AC objetivo ~100)
  (18, 'te_seed_p15_a', 15, 8, 'Generación reporte EVM',      '2026-03-15 09:00:00', '2026-03-15 18:00:00', 100.000, 1, NULL, NULL, '2026-03-15 18:00:00', '2026-03-15 18:00:00'),

  -- Proyecto 16 (BAC 1050, AC objetivo ~600)
  (19, 'te_seed_p16_a', 16, 5, 'Onboarding flujo principal',  '2026-01-10 09:00:00', '2026-01-10 18:00:00', 200.000, 1, NULL, NULL, '2026-01-10 18:00:00', '2026-01-10 18:00:00'),
  (20, 'te_seed_p16_b', 16, 6, 'Onboarding flujo secundario', '2026-02-12 09:00:00', '2026-02-12 18:00:00', 200.000, 1, NULL, NULL, '2026-02-12 18:00:00', '2026-02-12 18:00:00'),
  (21, 'te_seed_p16_c', 16, 8, 'Validaciones biométricas',    '2026-03-25 09:00:00', '2026-03-25 18:00:00', 200.000, 1, NULL, NULL, '2026-03-25 18:00:00', '2026-03-25 18:00:00'),

  -- Proyecto 17 (BAC 740, AC objetivo ~740 — cerrado, ETC=0)
  (22, 'te_seed_p17_a', 17, 7, 'Implementación dashboard',    '2025-10-15 09:00:00', '2025-10-15 18:00:00', 240.000, 1, NULL, NULL, '2025-10-15 18:00:00', '2025-10-15 18:00:00'),
  (23, 'te_seed_p17_b', 17, 5, 'Integración GA4',             '2026-01-20 09:00:00', '2026-01-20 18:00:00', 250.000, 1, NULL, NULL, '2026-01-20 18:00:00', '2026-01-20 18:00:00'),
  (24, 'te_seed_p17_c', 17, 8, 'Hardening + deploy',          '2026-03-30 09:00:00', '2026-03-30 18:00:00', 250.000, 1, NULL, NULL, '2026-03-30 18:00:00', '2026-03-30 18:00:00'),

  -- Proyecto 18 (BAC 250, AC objetivo ~50 — recién arrancado)
  (25, 'te_seed_p18_a', 18, 6, 'Kickoff y planificación',     '2026-03-05 09:00:00', '2026-03-05 18:00:00', 50.000,  1, NULL, NULL, '2026-03-05 18:00:00', '2026-03-05 18:00:00');

-- ---------------------------------------------------------------------
-- etc_snapshots (uno por proyecto, version=1, vigente)
-- ---------------------------------------------------------------------
INSERT INTO `etc_snapshots` (`id`, `project_id`, `version`, `label`, `created_at`, `updated_at`) VALUES
  (5,  11, 1, 'Snapshot inicial', '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (6,  12, 1, 'Snapshot inicial', '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (7,  13, 1, 'Snapshot inicial', '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (8,  14, 1, 'Snapshot inicial', '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (9,  15, 1, 'Snapshot inicial', '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (10, 16, 1, 'Snapshot inicial', '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (11, 17, 1, 'Snapshot cierre',  '2026-03-31 09:00:00', '2026-03-31 09:00:00'),
  (12, 18, 1, 'Snapshot inicial', '2026-04-01 09:00:00', '2026-04-01 09:00:00');

-- ---------------------------------------------------------------------
-- etc_records (sumados por proyecto = ETC esperado)
-- ---------------------------------------------------------------------
INSERT INTO `etc_records`
  (`id`, `project_id`, `snapshot_id`, `user_id`, `user_name`,
   `month_key`, `month_label`, `hours`, `created_at`, `updated_at`)
VALUES
  -- Proyecto 11 — ETC total 200
  (14, 11, 5,  5, 'daniel.alcazar',     '2026-05', 'Mayo 2026',     100.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (15, 11, 5,  6, 'diegolanus89',       '2026-06', 'Junio 2026',    100.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),

  -- Proyecto 12 — ETC total 500
  (16, 12, 6,  5, 'daniel.alcazar',     '2026-05', 'Mayo 2026',     180.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (17, 12, 6,  6, 'diegolanus89',       '2026-06', 'Junio 2026',    160.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (18, 12, 6,  7, 'haunau.lucia',       '2026-07', 'Julio 2026',    160.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),

  -- Proyecto 13 — ETC total 100
  (19, 13, 7,  8, 'santiagoguerci96',   '2026-05', 'Mayo 2026',     100.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),

  -- Proyecto 14 — ETC total 250
  (20, 14, 8,  5, 'daniel.alcazar',     '2026-05', 'Mayo 2026',     125.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (21, 14, 8,  7, 'haunau.lucia',       '2026-06', 'Junio 2026',    125.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),

  -- Proyecto 15 — ETC total 150
  (22, 15, 9,  8, 'santiagoguerci96',   '2026-05', 'Mayo 2026',      75.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (23, 15, 9,  6, 'diegolanus89',       '2026-06', 'Junio 2026',     75.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),

  -- Proyecto 16 — ETC total 300
  (24, 16, 10, 5, 'daniel.alcazar',     '2026-05', 'Mayo 2026',     150.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (25, 16, 10, 8, 'santiagoguerci96',   '2026-06', 'Junio 2026',    150.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),

  -- Proyecto 17 — ETC 0 (proyecto cerrado, no se cargan records)

  -- Proyecto 18 — ETC total 200
  (26, 18, 12, 6, 'diegolanus89',       '2026-05', 'Mayo 2026',     100.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00'),
  (27, 18, 12, 7, 'haunau.lucia',       '2026-06', 'Junio 2026',    100.00, '2026-04-01 09:00:00', '2026-04-01 09:00:00');

-- ---------------------------------------------------------------------
-- project_trackings
-- Cobertura:
--   p11 -> tracking + 1 update              -> "Sí (1)"
--   p12 -> tracking + 3 updates             -> "Sí (3)"
--   p13 -> SIN tracking en DB               -> "No"
--   p14 -> tracking + 2 updates             -> "Sí (2)"
--   p15 -> tracking + 0 updates             -> "No"
--   p16 -> tracking + 4 updates             -> "Sí (4)"
--   p17 -> tracking + 1 update              -> "Sí (1)"
--   p18 -> tracking + 0 updates             -> "No"
-- ---------------------------------------------------------------------
INSERT INTO `project_trackings`
  (`id`, `project_id`, `start_date`, `planned_end_date`, `actual_end_date`, `implementation_date`, `created_at`, `updated_at`)
VALUES
  (3, 11, '2026-01-15', '2026-07-30', NULL,         '2026-08-05', '2026-01-15 10:00:00', '2026-04-20 10:00:00'),
  (4, 12, '2025-11-01', '2026-12-31', NULL,         '2027-01-10', '2025-11-01 10:00:00', '2026-04-21 10:00:00'),
  (5, 14, '2026-01-05', '2026-08-20', NULL,         '2026-08-30', '2026-01-05 10:00:00', '2026-04-15 10:00:00'),
  (6, 15, '2026-03-01', '2026-09-30', NULL,         '2026-10-05', '2026-03-01 10:00:00', '2026-04-10 10:00:00'),
  (7, 16, '2025-12-01', '2026-10-31', NULL,         '2026-11-15', '2025-12-01 10:00:00', '2026-04-25 10:00:00'),
  (8, 17, '2025-08-01', '2026-03-31', '2026-04-05', '2026-04-05', '2025-08-01 10:00:00', '2026-04-05 18:00:00'),
  (9, 18, '2026-02-20', '2026-11-30', NULL,         '2026-12-10', '2026-02-20 10:00:00', '2026-04-12 10:00:00');

-- ---------------------------------------------------------------------
-- project_tracking_updates (historial de desvíos / demoras)
-- ---------------------------------------------------------------------
INSERT INTO `project_tracking_updates`
  (`id`, `project_tracking_id`, `change_end_date`, `observations`, `created_at`, `updated_at`)
VALUES
  -- Tracking 3 (proyecto 11) — 1 update
  (3,  3, '2026-08-15', 'Demora por validación de seguridad del cliente.',                                  '2026-04-10 14:30:00', '2026-04-10 14:30:00'),

  -- Tracking 4 (proyecto 12) — 3 updates
  (4,  4, '2026-12-15', 'Reestimación luego de revisión técnica de arquitectura.',                          '2026-02-15 09:00:00', '2026-02-15 09:00:00'),
  (5,  4, '2027-01-20', 'Pedido de alcance adicional aprobado por sponsor.',                                '2026-03-20 16:45:00', '2026-03-20 16:45:00'),
  (6,  4, '2027-02-28', 'Migración de datos masiva agrega 6 semanas al cronograma.',                        '2026-04-18 11:20:00', '2026-04-18 11:20:00'),

  -- Tracking 5 (proyecto 14) — 2 updates
  (7,  5, '2026-09-10', 'Auditor externo solicita evidencia adicional sobre RBAC.',                         '2026-03-05 10:00:00', '2026-03-05 10:00:00'),
  (8,  5, '2026-10-05', 'Re-test de pruebas de penetración pospuesto por agenda del proveedor.',            '2026-04-12 15:00:00', '2026-04-12 15:00:00'),

  -- Tracking 7 (proyecto 16) — 4 updates
  (9,  7, '2026-04-30', 'Pausa por priorización de release crítico.',                                       '2026-03-01 09:30:00', '2026-03-01 09:30:00'),
  (10, 7, '2026-06-15', 'Reanudación con equipo reducido.',                                                 '2026-04-05 11:00:00', '2026-04-05 11:00:00'),
  (11, 7, '2026-09-30', 'Cliente solicita reescritura del flujo de onboarding secundario.',                 '2026-04-15 14:00:00', '2026-04-15 14:00:00'),
  (12, 7, '2026-12-20', 'Integración biométrica requiere certificación adicional.',                         '2026-04-25 16:30:00', '2026-04-25 16:30:00'),

  -- Tracking 8 (proyecto 17) — 1 update
  (13, 8, '2026-04-05', 'Cierre formal con 5 días de demora respecto a planificado.',                       '2026-04-05 17:00:00', '2026-04-05 17:00:00');

-- =====================================================================
-- FIN seed RF-10
-- =====================================================================
