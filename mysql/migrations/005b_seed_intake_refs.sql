-- ============================================================
-- 005b_seed_intake_refs.sql
-- Seed de tablas de referencia para Intake (RF-04 / RF-07).
-- Idempotente: INSERT IGNORE (no duplica si ya existen).
-- Requiere que 005_add_intake_tables.sql ya se haya aplicado.
-- ============================================================

USE pm_timesheet_evm;

INSERT IGNORE INTO `project_intake_type_refs`
  (`code`, `label`, `description`, `internal_label`, `secondary_label`,
   `registration_label`, `requires_business_status_date`,
   `requires_actual_end_date`, `requires_commercial_fields`, `is_active`,
   `created_at`, `updated_at`)
VALUES
  ('30', 'Desarrollo', 'Proyectos de desarrollo de software',
   'Nro. Proyecto Desarrollo', 'Nro. Proyecto Comercial', 'Fecha de Alta',
   1, 0, 0, 1, NOW(), NOW());

INSERT IGNORE INTO `project_intake_category_refs`
  (`code`, `label`, `description`, `is_active`, `created_at`, `updated_at`)
VALUES
  ('PRE',   'Pre-venta',          'Proyectos en etapa de preventa',            1, NOW(), NOW()),
  ('DES',   'Desarrollo',         'Proyectos en desarrollo activo',            1, NOW(), NOW()),
  ('SOP',   'Soporte',            'Proyectos en etapa de soporte',             1, NOW(), NOW()),
  ('I+D',   'I+D',                'Investigación y desarrollo',                1, NOW(), NOW()),
  ('SWF',   'SWFactory',          'Proyectos bajo modalidad SW Factory',       1, NOW(), NOW()),
  ('DEV',   'DevOps',             'Proyectos de infraestructura y DevOps',     1, NOW(), NOW()),
  ('STAFF', 'Staff Augmentation', 'Proyectos de staffing',                     1, NOW(), NOW()),
  ('PROXY', 'Proxy Int.',         'Proyectos de proxy internacional',          1, NOW(), NOW());

INSERT IGNORE INTO `project_intake_status_refs`
  (`code`, `label`, `description`, `is_active`, `created_at`, `updated_at`)
VALUES
  ('INGRESO',    'Ingreso',    'Proyecto ingresado y en evaluación',        1, NOW(), NOW()),
  ('EN_CURSO',   'En curso',   'Proyecto aprobado y en ejecución',          1, NOW(), NOW()),
  ('PERDIDO',    'Perdido',    'Proyecto no adjudicado o cancelado',        1, NOW(), NOW()),
  ('CERRADO',    'Cerrado',    'Proyecto finalizado y cerrado formalmente', 1, NOW(), NOW()),
  ('SUSPENDIDO', 'Suspendido', 'Proyecto pausado temporalmente',            1, NOW(), NOW());
