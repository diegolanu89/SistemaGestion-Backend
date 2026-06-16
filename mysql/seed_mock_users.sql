-- ============================================================
-- Seed de usuarios mock para pruebas locales
-- Todos con password: 123456
-- BCrypt ($2b$12$...) — compatible con BCrypt.Net del backend
-- Re-ejecutable: ON DUPLICATE KEY UPDATE actualiza si el email ya existe
-- ============================================================
-- Uso:
--   docker compose exec -T db mysql -u bdt_user -pbdt_user pm_timesheet_evm < SistemaGestion-Backend/mysql/seed_mock_users.sql
-- (ajustar el nombre de la base si corresponde: pm_timesheet_evm)
-- ============================================================

SET @pwd = '$2b$12$2AoLUG1kj9CYIUo1Re0pMuR1Wfj5XyPaY/ifzUCDOOj6pe7Hrl1CS'; -- 123456

INSERT INTO `users` (`name`, `email`, `password`, `profile_id`, `active`, `created_at`, `updated_at`) VALUES
  ('Mock Administrador',     'mock.admin@test.com',          @pwd, 1, 1, NOW(), NOW()),
  ('Mock Soporte',           'mock.soporte@test.com',        @pwd, 2, 1, NOW(), NOW()),
  ('Mock Ops Gerente',       'mock.ops.gerente@test.com',    @pwd, 3, 1, NOW(), NOW()),
  ('Mock Ops Lider',         'mock.ops.lider@test.com',      @pwd, 4, 1, NOW(), NOW()),
  ('Mock Administracion',    'mock.administracion@test.com', @pwd, 5, 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE
  `password`   = VALUES(`password`),
  `profile_id` = VALUES(`profile_id`),
  `active`     = VALUES(`active`),
  `updated_at` = NOW();
