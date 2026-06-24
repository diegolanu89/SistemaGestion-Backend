-- ============================================================
-- 03_seed_mock.sql — Datos mock para desarrollo y demo.
-- Solo para entorno dev. No ejecutar en producción.
-- ============================================================

USE pm_timesheet_evm;
SET NAMES utf8mb4;

-- ── Clientes ──────────────────────────────────────────────────

INSERT INTO `timesheet_clients` (`id`, `name`, `external_id`, `status`, `created_at`, `updated_at`) VALUES
  (3, 'BDT Global',            '69dd703ead41c81887a57f77', 'activo', '2026-04-20 16:37:24', '2026-04-27 19:55:13'),
  (4, 'Cliente Test 1',        '69dd71cf6f055ba5e9dd2f7f', 'activo', '2026-04-20 16:37:24', '2026-04-27 19:55:13'),
  (5, 'Google Inc',            '69e67b9070d51f23d4525390', 'activo', '2026-04-21 21:10:52', '2026-04-27 19:55:13'),
  (6, 'Anthropic Testing Corp','69efbecf9b95089258bb1d51', 'activo', '2026-04-27 19:55:13', '2026-04-27 19:55:13');

-- ── Usuarios de timesheet ─────────────────────────────────────

INSERT INTO `timesheet_users`
  (`id`, `timesheet_user_id`, `name`, `email`, `role`, `active`, `default_month_hours`, `created_at`, `updated_at`)
VALUES
  ( 4, '69dabb567617f51a95898fe6', 'Usuario sin nombre', 'christian.bass221@gmail.com', NULL, 0,   NULL, '2026-04-12 22:38:17', '2026-04-21 21:10:54'),
  ( 5, '69dabb567617f51a95898fe4', 'daniel.alcazar',     'danielalcazar54@gmail.com',   NULL, 1,   NULL, '2026-04-12 22:38:17', '2026-04-21 21:10:54'),
  ( 6, '69dabb567617f51a95898fe8', 'diegolanus89',       'diegolanus89@gmail.com',      NULL, 1,   NULL, '2026-04-12 22:38:17', '2026-04-21 21:10:54'),
  ( 7, '69dabb567617f51a95898fe5', 'haunau.lucia',       'haunau.lucia@gmail.com',      NULL, 1,   NULL, '2026-04-12 22:38:17', '2026-04-21 21:10:54'),
  ( 8, '69dab2787617f51a958934e1', 'santiagoguerci96',   'santiagoguerci96@gmail.com',  NULL, 1,   NULL, '2026-04-12 22:38:17', '2026-04-21 21:10:54');

-- ── Usuarios del sistema (contraseña: "password" en BCrypt) ──

INSERT INTO `users` (`id`, `name`, `email`, `email_verified_at`, `password`, `profile_id`, `active`, `remember_token`, `created_at`, `updated_at`) VALUES
  (1, 'Admin',     'admin@evm.com',   NULL, '$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy', 1, 1, NULL, NULL, NULL),
  (2, 'Daniel',    'daniel@test.com', NULL, '$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy', 1, 1, NULL, NULL, NULL),
  (3, 'Diego',     'diego@test.com',  NULL, '$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy', 1, 1, NULL, NULL, NULL),
  (4, 'Lucia',     'lucia@test.com',  NULL, '$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy', 1, 1, NULL, NULL, NULL),
  (5, 'Santiago',  'santi@test.com',  NULL, '$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy', 1, 1, NULL, NULL, NULL),
  (6, 'Christian', 'chris@test.com',  NULL, '$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy', 1, 1, NULL, NULL, NULL);

-- ── Capacidad y RRHH ──────────────────────────────────────────

INSERT INTO `user_leaders` (`id`, `user_id`, `leader_id`, `start_date`, `end_date`, `notes`, `created_at`, `updated_at`) VALUES
  ( 3, 5, 6, '2026-01-01', '2026-04-21', 'Asignación inicial', '2026-04-21 00:16:09', '2026-04-21 00:17:54'),
  ( 4, 7, 6, '2026-04-21', '2026-04-21', NULL,                 '2026-04-21 00:16:30', '2026-04-21 00:18:18'),
  ( 6, 5, 7, '2026-01-01', NULL,          NULL,                 '2026-04-21 00:17:54', '2026-04-21 00:17:54'),
  ( 7, 6, 7, '2026-01-01', '2026-04-21', NULL,                 '2026-04-21 00:17:54', '2026-04-21 00:18:18'),
  ( 8, 7, 5, '2026-01-01', NULL,          NULL,                 '2026-04-21 00:18:18', '2026-04-21 00:18:18'),
  ( 9, 6, 5, '2026-01-01', NULL,          NULL,                 '2026-04-21 00:18:18', '2026-04-21 00:18:18'),
  (10, 8, 5, '2025-01-01', NULL,          'Asignación inicial', '2026-04-21 17:29:19', '2026-04-21 17:29:19');

INSERT INTO `user_monthly_capacities` (`id`, `user_id`, `month_key`, `month_label`, `hours`, `created_at`, `updated_at`) VALUES
  (1, 5, '2026-01', 'Enero 2026',  120.00, '2026-04-21 01:44:55', '2026-04-21 01:45:16'),
  (3, 5, '2026-03', 'Marzo 2026',  168.00, '2026-04-21 01:44:55', '2026-04-21 01:44:55'),
  (4, 5, '2026-04', 'Abril 2026',  176.00, '2026-04-21 01:45:29', '2026-04-21 01:45:29'),
  (5, 8, '2025-05', 'Mayo 2025',   140.00, '2026-04-21 17:44:03', '2026-04-21 17:44:03'),
  (6, 8, '2025-06', 'Junio 2025',  160.00, '2026-04-21 17:44:03', '2026-04-21 17:44:03'),
  (7, 8, '2025-07', 'Julio 2025',  120.00, '2026-04-21 17:44:03', '2026-04-21 17:44:03');

INSERT INTO `user_vacation_periods` (`id`, `user_id`, `date_from`, `date_to`, `total_days`, `notes`, `created_at`, `updated_at`) VALUES
  (1, 5, '2024-05-01', '2024-05-15', 15, 'Vacaciones de invierno', '2026-04-15 15:26:35', '2026-04-15 15:26:35'),
  (2, 7, '2026-05-01', '2026-05-10', 10, 'Vacaciones de mayo',     '2026-04-21 02:08:07', '2026-04-21 02:08:07'),
  (4, 6, '2026-06-01', '2026-06-05',  5, NULL,                     '2026-04-21 02:08:33', '2026-04-21 02:08:33'),
  (5, 8, '2025-07-01', '2025-07-15', 15, 'Vacaciones de verano',   '2026-04-21 18:58:38', '2026-04-21 18:58:38');

-- ── Pipeline ──────────────────────────────────────────────────

INSERT INTO `potencial_clients` (`id`, `name`, `created_at`, `updated_at`) VALUES
  (2, 'Banco Galicia', '2026-04-16 21:56:52', '2026-04-16 21:56:52');

