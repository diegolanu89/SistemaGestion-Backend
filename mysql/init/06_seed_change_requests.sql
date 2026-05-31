-- =====================================================================
-- 06_seed_change_requests.sql
-- Seed de change_requests para los 8 proyectos cargados por 05_seed_evm.sql.
--
-- Cubre los casos visuales del Dashboard EVM (RF-10) para "Control de cambios":
--   p11 (Sistema Cotizador Web)         -> Sí (1)
--   p12 (Migración Core BDT)            -> Sí (3)
--   p13 (Integración Gemini API)        -> No
--   p14 (Auditoría Compliance Anthropic)-> Sí (2)
--   p15 (Reporte EVM Mensual)           -> No
--   p16 (Onboarding Digital v2)         -> Sí (4)
--   p17 (Dashboard Analytics Google)    -> Sí (1)
--   p18 (Plataforma Soporte Anthropic)  -> No
--
-- IMPORTANTE: ProjectBacService.RecalculateTotal hace
--   bac_total = bac_base + sum(bac_hours_increment WHERE > 0)
-- y se ejecuta en cada GET /api/projects[/evm]. Para que los bac_total ya
-- cargados por 05_seed_evm.sql queden consistentes con los increments de
-- estos CRs, se ajusta bac_base del proyecto 14 (que en 05 tenía
-- bac_base=600 y bac_total=600, ahora pasa a bac_base=540 + 60 hs de CRs).
-- =====================================================================

USE pm_clockify_evm;

-- p14: bac_base 600 -> 540 (con 2 CRs de 30 hs cada uno, bac_total queda 600 igual que antes)
UPDATE `clockify_projects`
   SET `bac_base_hours` = 540.00,
       `bac_base_cost`  = 32400.00,
       `updated_at`     = NOW()
 WHERE `id` = 14;

INSERT INTO `change_requests`
  (`id`, `project_id`, `code`, `title`, `description`,
   `requested_by`, `requested_date`, `status`,
   `bac_hours_increment`, `bac_cost_increment`,
   `approved_by`, `approved_date`, `created_at`, `updated_at`)
VALUES
  -- ------- Proyecto 11 (Sistema Cotizador Web) -- 1 CR -------
  (8,  11, 'CC-001', 'Integración con Stripe',
       'Cliente solicita habilitar pago con tarjeta vía Stripe en el flujo de cotización.',
       'cliente.test1@example.com', '2026-02-20', 'aprobado',
       50.00, 3000.00,
       'admin@bdt.com', '2026-02-25', '2026-02-20 10:00:00', '2026-02-25 14:30:00'),

  -- ------- Proyecto 12 (Migración Core BDT) -- 3 CRs -------
  (9,  12, 'CC-001', 'Adapter legacy SOAP',
       'Conexión adicional con sistema legacy del cliente vía SOAP.',
       'cliente.bdt@example.com', '2026-01-15', 'aprobado',
       40.00, 2400.00,
       'admin@bdt.com', '2026-01-20', '2026-01-15 09:00:00', '2026-01-20 16:45:00'),

  (10, 12, 'CC-002', 'Encriptación at-rest reforzada',
       'Cumplimiento de normativa de seguridad bancaria local.',
       'cliente.bdt@example.com', '2026-02-10', 'aprobado',
       40.00, 2400.00,
       'admin@bdt.com', '2026-02-15', '2026-02-10 09:00:00', '2026-02-15 16:45:00'),

  (11, 12, 'CC-003', 'Pipeline CI dedicado',
       'Setup de pipeline de CI/CD dedicado a este proyecto. Pendiente de aprobación final.',
       'lider.tech@bdt.com', '2026-04-05', 'propuesto',
       40.00, 2400.00,
       NULL, NULL, '2026-04-05 11:00:00', '2026-04-05 11:00:00'),

  -- ------- Proyecto 14 (Auditoría Compliance Anthropic) -- 2 CRs -------
  (12, 14, 'CC-001', 'Documentación ISO 27001 extra',
       'Auditor solicita procedimientos documentados adicionales para sección A.12.',
       'auditor@anthropic.com', '2026-02-15', 'aprobado',
       30.00, 1800.00,
       'admin@bdt.com', '2026-02-22', '2026-02-15 10:00:00', '2026-02-22 17:00:00'),

  (13, 14, 'CC-002', 'Penetration test re-take',
       'Pen-test postergado por agenda del proveedor, se ejecuta en sprint adicional.',
       'auditor@anthropic.com', '2026-03-20', 'aprobado',
       30.00, 1800.00,
       'admin@bdt.com', '2026-03-28', '2026-03-20 09:00:00', '2026-03-28 11:30:00'),

  -- ------- Proyecto 16 (Onboarding Digital v2) -- 4 CRs -------
  (14, 16, 'CC-001', 'Validación biométrica iOS',
       'Implementación de validación biométrica para iOS además de Android.',
       'cliente.test1@example.com', '2026-01-20', 'aprobado',
       25.00, 1500.00,
       'admin@bdt.com', '2026-01-27', '2026-01-20 09:00:00', '2026-01-27 15:00:00'),

  (15, 16, 'CC-002', 'Validación biométrica Android',
       'Reglas biométricas específicas para Android 12+.',
       'cliente.test1@example.com', '2026-02-12', 'aprobado',
       25.00, 1500.00,
       'admin@bdt.com', '2026-02-18', '2026-02-12 09:00:00', '2026-02-18 14:00:00'),

  (16, 16, 'CC-003', 'Refactor flujo onboarding secundario',
       'El cliente solicita rediseñar el flujo secundario para mejorar usabilidad.',
       'cliente.test1@example.com', '2026-03-10', 'aprobado',
       25.00, 1500.00,
       'admin@bdt.com', '2026-03-18', '2026-03-10 09:00:00', '2026-03-18 10:00:00'),

  (17, 16, 'CC-004', 'Reintentos y manejo de error de carga DNI',
       'Se proponen reintentos automáticos en carga de DNI cuando hay timeout de red.',
       'lider.ux@bdt.com', '2026-04-15', 'propuesto',
       25.00, 1500.00,
       NULL, NULL, '2026-04-15 11:00:00', '2026-04-15 11:00:00'),

  -- ------- Proyecto 17 (Dashboard Analytics Google) -- 1 CR -------
  (18, 17, 'CC-001', 'Integración GA4 paid',
       'Cliente solicita habilitar reporting de campañas pagas en GA4.',
       'cliente.google@example.com', '2025-11-20', 'implementado',
       40.00, 2400.00,
       'admin@bdt.com', '2025-11-28', '2025-11-20 10:00:00', '2026-02-15 16:00:00');

-- =====================================================================
-- FIN seed change requests
-- =====================================================================
