-- MySQL dump 10.13  Distrib 8.0.45, for Linux (x86_64)
--
-- Host: localhost    Database: pm_clockify_evm
-- ------------------------------------------------------
-- Server version	8.0.45

CREATE DATABASE IF NOT EXISTS pm_clockify_evm
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE pm_clockify_evm;

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Estructura de la tabla `app_user_visible_projects`
--

DROP TABLE IF EXISTS `app_user_visible_projects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `app_user_visible_projects` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `project_id` bigint unsigned NOT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `app_user_visible_projects_user_id_project_id_unique` (`user_id`,`project_id`),
  CONSTRAINT `app_user_visible_projects_user_id_foreign` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `app_user_visible_projects`
--

LOCK TABLES `app_user_visible_projects` WRITE;
/*!40000 ALTER TABLE `app_user_visible_projects` DISABLE KEYS */;
/*!40000 ALTER TABLE `app_user_visible_projects` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `change_requests`
--

DROP TABLE IF EXISTS `change_requests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `change_requests` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `project_id` bigint unsigned NOT NULL,
  `code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `title` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text COLLATE utf8mb4_unicode_ci,
  `requested_by` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `requested_date` date NOT NULL,
  `status` enum('propuesto','aprobado','rechazado','implementado') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'propuesto',
  `bac_hours_increment` decimal(10,2) NOT NULL DEFAULT '0.00',
  `bac_cost_increment` decimal(12,2) NOT NULL DEFAULT '0.00',
  `approved_by` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `approved_date` date DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_cr_project_code` (`project_id`,`code`),
  KEY `idx_cr_status` (`status`),
  KEY `idx_cr_requested_date` (`requested_date`),
  CONSTRAINT `change_requests_project_id_foreign` FOREIGN KEY (`project_id`) REFERENCES `clockify_projects` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `change_requests`
--

LOCK TABLES `change_requests` WRITE;
/*!40000 ALTER TABLE `change_requests` DISABLE KEYS */;
/*!40000 ALTER TABLE `change_requests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `clockify_clients`
--

DROP TABLE IF EXISTS `clockify_clients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clockify_clients` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `external_id` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `status` enum('activo','inactivo') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'activo',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_clients_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `clockify_clients`
--

LOCK TABLES `clockify_clients` WRITE;
/*!40000 ALTER TABLE `clockify_clients` DISABLE KEYS */;
INSERT INTO `clockify_clients` VALUES (3,'BDT Global','69dd703ead41c81887a57f77','activo','2026-04-20 16:37:24','2026-04-27 19:55:13'),(4,'Cliente Test 1','69dd71cf6f055ba5e9dd2f7f','activo','2026-04-20 16:37:24','2026-04-27 19:55:13'),(5,'Google Inc','69e67b9070d51f23d4525390','activo','2026-04-21 21:10:52','2026-04-27 19:55:13'),(6,'Anthropic Testing Corp','69efbecf9b95089258bb1d51','activo','2026-04-27 19:55:13','2026-04-27 19:55:13');
/*!40000 ALTER TABLE `clockify_clients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `clockify_project_filters`
--

DROP TABLE IF EXISTS `clockify_project_filters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clockify_project_filters` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `project_id` bigint unsigned NOT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_project_filters_project` (`project_id`),
  CONSTRAINT `clockify_project_filters_project_id_foreign` FOREIGN KEY (`project_id`) REFERENCES `clockify_projects` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `clockify_project_filters`
--

LOCK TABLES `clockify_project_filters` WRITE;
/*!40000 ALTER TABLE `clockify_project_filters` DISABLE KEYS */;
INSERT INTO `clockify_project_filters` VALUES (4,3,'2026-04-21 18:00:14','2026-04-21 18:00:14');
/*!40000 ALTER TABLE `clockify_project_filters` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `clockify_projects`
--

DROP TABLE IF EXISTS `clockify_projects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clockify_projects` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `clockify_project_id` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `code` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `client_id` bigint unsigned DEFAULT NULL,
  `status` enum('activo','pausado','cerrado') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'activo',
  `start_date` date DEFAULT NULL,
  `end_date_planned` date DEFAULT NULL,
  `end_date_actual` date DEFAULT NULL,
  `bac_base_hours` decimal(10,2) NOT NULL DEFAULT '0.00',
  `bac_base_cost` decimal(12,2) NOT NULL DEFAULT '0.00',
  `bac_total_hours` decimal(10,2) NOT NULL DEFAULT '0.00',
  `bac_total_cost` decimal(12,2) NOT NULL DEFAULT '0.00',
  `hourly_rate` decimal(10,2) NOT NULL DEFAULT '0.00',
  `etc_calculation_mode` enum('manual','automatic') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'manual',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_projects_clockify` (`clockify_project_id`),
  KEY `idx_projects_client` (`client_id`),
  KEY `idx_projects_status` (`status`),
  CONSTRAINT `clockify_projects_client_id_foreign` FOREIGN KEY (`client_id`) REFERENCES `clockify_clients` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `clockify_projects`
--

LOCK TABLES `clockify_projects` WRITE;
/*!40000 ALTER TABLE `clockify_projects` DISABLE KEYS */;
INSERT INTO `clockify_projects` VALUES (3,'69dd7043ebaf76dec4b0a334','Gestion de Proyectos','',3,'activo',NULL,NULL,NULL,0.00,0.00,0.00,0.00,0.00,'manual','2026-04-20 16:54:37','2026-04-21 21:10:53'),(4,'69dd723debaf76dec4b0d4ea','Migraci├│n','',3,'activo',NULL,NULL,NULL,0.00,0.00,0.00,0.00,0.00,'manual','2026-04-20 16:54:37','2026-04-21 21:10:53'),(5,'69dd71d1ebaf76dec4b0cade','Proyecto Test 1','',4,'activo',NULL,NULL,NULL,0.00,0.00,0.00,0.00,0.00,'manual','2026-04-20 16:54:37','2026-04-21 21:10:53'),(6,'69e67bcc70d51f23d4525e9f','Gemini','',5,'activo',NULL,NULL,NULL,0.00,0.00,0.00,0.00,0.00,'manual','2026-04-21 21:10:53','2026-04-21 21:10:53'),(7,'69efbc4e9e6197e396433793','Plataforma IA Interna',NULL,NULL,'activo',NULL,NULL,NULL,0.00,0.00,0.00,0.00,0.00,'manual','2026-04-27 19:43:12','2026-04-27 19:43:12'),(8,'69efc4a403616156f982d73e','Plataforma IA Interna Audit',NULL,NULL,'activo',NULL,NULL,NULL,0.00,0.00,0.00,0.00,0.00,'manual','2026-04-27 20:18:46','2026-04-27 20:18:46');
/*!40000 ALTER TABLE `clockify_projects` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `clockify_time_entries`
--

DROP TABLE IF EXISTS `clockify_time_entries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clockify_time_entries` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `clockify_time_entry_id` varchar(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `project_id` bigint unsigned NOT NULL,
  `user_id` bigint unsigned DEFAULT NULL,
  `description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `start_time` datetime NOT NULL,
  `end_time` datetime NOT NULL,
  `duration_hours` decimal(10,3) NOT NULL,
  `billable` tinyint(1) NOT NULL DEFAULT '1',
  `change_request_id` bigint unsigned DEFAULT NULL,
  `source_raw` json DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_te_clockify` (`clockify_time_entry_id`),
  KEY `idx_te_project` (`project_id`),
  KEY `idx_te_user` (`user_id`),
  KEY `idx_te_start_time` (`start_time`),
  KEY `idx_te_cr` (`change_request_id`),
  CONSTRAINT `clockify_time_entries_change_request_id_foreign` FOREIGN KEY (`change_request_id`) REFERENCES `change_requests` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `clockify_time_entries_project_id_foreign` FOREIGN KEY (`project_id`) REFERENCES `clockify_projects` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `clockify_time_entries_user_id_foreign` FOREIGN KEY (`user_id`) REFERENCES `clockify_users` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_te_user` FOREIGN KEY (`user_id`) REFERENCES `clockify_users` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `clockify_time_entries`
--

LOCK TABLES `clockify_time_entries` WRITE;
/*!40000 ALTER TABLE `clockify_time_entries` DISABLE KEYS */;
INSERT INTO `clockify_time_entries` VALUES (3,'69dd72baebaf76dec4b0e0c5',3,8,'','2026-04-14 12:00:00','2026-04-14 17:00:00',5.000,1,NULL,'{\"id\": \"69dd72baebaf76dec4b0e0c5\", \"type\": \"REGULAR\", \"tagIds\": null, \"taskId\": null, \"userId\": \"69dab2787617f51a958934e1\", \"kioskId\": null, \"billable\": true, \"costRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"isLocked\": false, \"projectId\": \"69dd7043ebaf76dec4b0a334\", \"hourlyRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"description\": \"\", \"workspaceId\": \"69dab2777617f51a958934da\", \"timeInterval\": {\"end\": \"2026-04-14T17:00:00Z\", \"start\": \"2026-04-14T12:00:00Z\", \"duration\": \"PT5H\"}, \"customFieldValues\": []}','2026-04-20 17:12:52','2026-04-20 18:47:04'),(4,'69dd72db07713d77a7ccbab8',3,8,'Migraci├│n de PHP a .NET','2026-04-13 22:48:59','2026-04-13 22:49:09',0.003,1,NULL,'{\"id\": \"69dd72db07713d77a7ccbab8\", \"type\": \"REGULAR\", \"tagIds\": null, \"taskId\": null, \"userId\": \"69dab2787617f51a958934e1\", \"kioskId\": null, \"billable\": true, \"costRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"isLocked\": false, \"projectId\": \"69dd7043ebaf76dec4b0a334\", \"hourlyRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"description\": \"Migraci├│n de PHP a .NET\", \"workspaceId\": \"69dab2777617f51a958934da\", \"timeInterval\": {\"end\": \"2026-04-13T22:49:09Z\", \"start\": \"2026-04-13T22:48:59Z\", \"duration\": \"PT10S\"}, \"customFieldValues\": []}','2026-04-20 17:12:52','2026-04-20 18:47:04'),(5,'69dd72b382fef641e54c79b8',3,8,'Revision proyecto','2026-04-13 12:00:00','2026-04-13 21:00:00',9.000,1,NULL,'{\"id\": \"69dd72b382fef641e54c79b8\", \"type\": \"REGULAR\", \"tagIds\": null, \"taskId\": null, \"userId\": \"69dab2787617f51a958934e1\", \"kioskId\": null, \"billable\": true, \"costRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"isLocked\": false, \"projectId\": \"69dd7043ebaf76dec4b0a334\", \"hourlyRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"description\": \"Revision proyecto\", \"workspaceId\": \"69dab2777617f51a958934da\", \"timeInterval\": {\"end\": \"2026-04-13T21:00:00Z\", \"start\": \"2026-04-13T12:00:00Z\", \"duration\": \"PT9H\"}, \"customFieldValues\": []}','2026-04-20 17:12:52','2026-04-20 18:47:04'),(6,'69e677dfccba2f1f7bcc8680',4,5,'endpoints parte 1','2026-04-20 19:00:47','2026-04-20 19:13:36',0.214,1,NULL,'{\"id\": \"69e677dfccba2f1f7bcc8680\", \"type\": \"REGULAR\", \"tagIds\": null, \"taskId\": null, \"userId\": \"69dabb567617f51a95898fe4\", \"kioskId\": null, \"billable\": true, \"costRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"isLocked\": false, \"projectId\": \"69dd723debaf76dec4b0d4ea\", \"hourlyRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"description\": \"endpoints parte 1\", \"workspaceId\": \"69dab2777617f51a958934da\", \"timeInterval\": {\"end\": \"2026-04-20T19:13:36Z\", \"start\": \"2026-04-20T19:00:47Z\", \"duration\": \"PT12M49S\"}, \"customFieldValues\": []}','2026-04-21 21:10:55','2026-04-21 21:10:55'),(7,'69e6771870d51f23d4517b67',4,5,'endpoints parte 1','2026-04-20 18:57:28','2026-04-20 18:58:40',0.020,1,NULL,'{\"id\": \"69e6771870d51f23d4517b67\", \"type\": \"REGULAR\", \"tagIds\": null, \"taskId\": null, \"userId\": \"69dabb567617f51a95898fe4\", \"kioskId\": null, \"billable\": true, \"costRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"isLocked\": false, \"projectId\": \"69dd723debaf76dec4b0d4ea\", \"hourlyRate\": {\"amount\": 0, \"currency\": \"USD\"}, \"description\": \"endpoints parte 1\", \"workspaceId\": \"69dab2777617f51a958934da\", \"timeInterval\": {\"end\": \"2026-04-20T18:58:40Z\", \"start\": \"2026-04-20T18:57:28Z\", \"duration\": \"PT1M12S\"}, \"customFieldValues\": []}','2026-04-21 21:10:55','2026-04-21 21:10:55');
/*!40000 ALTER TABLE `clockify_time_entries` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `clockify_users`
--

DROP TABLE IF EXISTS `clockify_users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clockify_users` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `clockify_user_id` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `email` varchar(191) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `role` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  `default_month_hours` decimal(8,2) DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_users_clockify` (`clockify_user_id`),
  KEY `idx_users_email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `clockify_users`
--

LOCK TABLES `clockify_users` WRITE;
/*!40000 ALTER TABLE `clockify_users` DISABLE KEYS */;
INSERT INTO `clockify_users` VALUES (4,'69dabb567617f51a95898fe6','Usuario sin nombre','christian.bass221@gmail.com',NULL,0,140.00,'2026-04-12 22:38:17','2026-04-21 21:10:54'),(5,'69dabb567617f51a95898fe4','daniel.alcazar','danielalcazar54@gmail.com',NULL,1,NULL,'2026-04-12 22:38:17','2026-04-21 21:10:54'),(6,'69dabb567617f51a95898fe8','diegolanus89','diegolanus89@gmail.com',NULL,1,NULL,'2026-04-12 22:38:17','2026-04-21 21:10:54'),(7,'69dabb567617f51a95898fe5','haunau.lucia','haunau.lucia@gmail.com',NULL,1,NULL,'2026-04-12 22:38:17','2026-04-21 21:10:54'),(8,'69dab2787617f51a958934e1','santiagoguerci96','santiagoguerci96@gmail.com',NULL,1,NULL,'2026-04-12 22:38:17','2026-04-21 21:10:54'),(15,'69dabb567617f51a95898fe7','Usuario sin nombre','aylenteresalee@gmail.com',NULL,0,NULL,'2026-04-21 21:10:54','2026-04-21 21:10:54');
/*!40000 ALTER TABLE `clockify_users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `etc_records`
--

DROP TABLE IF EXISTS `etc_records`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `etc_records` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `project_id` bigint unsigned NOT NULL,
  `snapshot_id` bigint unsigned DEFAULT NULL,
  `user_id` bigint unsigned DEFAULT NULL,
  `user_name` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `month_key` varchar(7) COLLATE utf8mb4_unicode_ci NOT NULL,
  `month_label` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `hours` decimal(10,2) NOT NULL DEFAULT '0.00',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_etc_project` (`project_id`),
  KEY `idx_etc_user` (`user_id`),
  KEY `idx_etc_month` (`month_key`),
  KEY `idx_etc_project_month` (`project_id`,`month_key`),
  KEY `idx_etc_project_user_month` (`project_id`,`user_id`,`month_key`),
  KEY `etc_records_snapshot_id_index` (`snapshot_id`),
  CONSTRAINT `etc_records_project_id_foreign` FOREIGN KEY (`project_id`) REFERENCES `clockify_projects` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `etc_records_snapshot_id_foreign` FOREIGN KEY (`snapshot_id`) REFERENCES `etc_snapshots` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `etc_records_user_id_foreign` FOREIGN KEY (`user_id`) REFERENCES `clockify_users` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `etc_records`
--

LOCK TABLES `etc_records` WRITE;
/*!40000 ALTER TABLE `etc_records` DISABLE KEYS */;
/*!40000 ALTER TABLE `etc_records` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `etc_snapshots`
--

DROP TABLE IF EXISTS `etc_snapshots`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `etc_snapshots` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `project_id` bigint unsigned NOT NULL,
  `version` int unsigned NOT NULL DEFAULT '1',
  `label` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `etc_snapshots_project_id_version_index` (`project_id`,`version`),
  CONSTRAINT `etc_snapshots_project_id_foreign` FOREIGN KEY (`project_id`) REFERENCES `clockify_projects` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `etc_snapshots`
--

LOCK TABLES `etc_snapshots` WRITE;
/*!40000 ALTER TABLE `etc_snapshots` DISABLE KEYS */;
/*!40000 ALTER TABLE `etc_snapshots` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `failed_jobs`
--

DROP TABLE IF EXISTS `failed_jobs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `failed_jobs` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `uuid` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `connection` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `queue` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `payload` longtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `exception` longtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `failed_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `failed_jobs_uuid_unique` (`uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `failed_jobs`
--

LOCK TABLES `failed_jobs` WRITE;
/*!40000 ALTER TABLE `failed_jobs` DISABLE KEYS */;
/*!40000 ALTER TABLE `failed_jobs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `migrations`
--

DROP TABLE IF EXISTS `migrations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `migrations` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `migration` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `batch` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=43 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `migrations`
--

LOCK TABLES `migrations` WRITE;
/*!40000 ALTER TABLE `migrations` DISABLE KEYS */;
INSERT INTO `migrations` VALUES (1,'2014_10_12_000000_create_users_table',1),(2,'2014_10_12_100000_create_password_reset_tokens_table',1),(3,'2019_08_19_000000_create_failed_jobs_table',1),(4,'2019_12_14_000001_create_personal_access_tokens_table',1),(5,'2025_11_19_120655_create_clients_table',1),(6,'2025_11_19_120658_create_clockify_users_table',1),(7,'2025_11_19_120701_create_projects_table',1),(8,'2025_11_19_120704_create_change_requests_table',1),(9,'2025_11_19_120707_create_time_entries_table',1),(10,'2025_11_19_120709_create_project_metrics_table',2),(11,'2025_11_19_120712_create_api_tokens_table',2),(12,'2025_11_19_120713_create_project_filters_table',2),(13,'2025_12_08_175731_create_etc_records_table',2),(14,'2025_12_08_180119_add_calculation_mode_to_projects_table',2),(15,'2025_12_09_204507_fix_time_entries_user_foreign_key',2),(16,'2025_12_10_180859_drop_api_tokens_table',2),(17,'2025_12_10_180921_drop_project_metrics_table',2),(18,'2025_12_10_180940_drop_users_table',2),(19,'2025_12_11_000000_recreate_users_table',2),(20,'2025_12_19_154300_increase_clockify_project_id_length',2),(21,'2026_01_06_200059_create_user_hours_summary_table',2),(22,'2026_01_06_200111_create_user_leaders_table',2),(23,'2026_01_06_200117_create_working_days_calendar_table',2),(24,'2026_01_06_200124_create_user_monthly_status_table',2),(25,'2026_01_13_212303_remove_note_and_is_visible_from_project_filters_table',2),(26,'2026_02_13_222626_replace_weekend_days_with_hours_month_in_working_days_calendar_table',2),(27,'2026_02_20_121023_rename_tables_with_clockify_prefix',2),(28,'2026_02_21_100000_create_etc_snapshots_and_add_snapshot_id_to_etc_records',2),(29,'2026_02_22_100000_ensure_clockify_table_names',2),(30,'2026_02_24_100000_create_potencial_clients_table',2),(31,'2026_02_24_100001_create_potencial_projects_table',2),(32,'2026_02_24_100002_create_potencial_project_allocations_table',2),(33,'2026_02_24_100003_add_indexes_potencial_project_allocations',2),(34,'2026_02_25_100000_create_user_vacation_periods_table',2),(35,'2026_02_26_100000_create_profiles_table',2),(36,'2026_02_26_100001_add_profile_and_active_to_users_table',2),(37,'2026_02_27_100000_create_app_user_visible_projects_table',2),(38,'2026_02_27_100001_create_user_dashboard_filters_table',2),(39,'2026_03_09_000000_add_profile_and_active_to_users_if_missing',2),(40,'2026_03_13_000410_add_role_and_default_month_hours_to_clockify_users_table',2),(41,'2026_03_13_000420_create_user_monthly_capacities_table',2),(42,'2026_03_13_000500_seed_user_monthly_capacities_from_working_days',2);
/*!40000 ALTER TABLE `migrations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `password_reset_tokens`
--

DROP TABLE IF EXISTS `password_reset_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `password_reset_tokens` (
  `email` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `token` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `password_reset_tokens`
--

LOCK TABLES `password_reset_tokens` WRITE;
/*!40000 ALTER TABLE `password_reset_tokens` DISABLE KEYS */;
/*!40000 ALTER TABLE `password_reset_tokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `personal_access_tokens`
--

DROP TABLE IF EXISTS `personal_access_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `personal_access_tokens` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `tokenable_type` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `tokenable_id` bigint unsigned NOT NULL,
  `name` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `token` varchar(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `abilities` text COLLATE utf8mb4_unicode_ci,
  `last_used_at` timestamp NULL DEFAULT NULL,
  `expires_at` timestamp NULL DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `personal_access_tokens_token_unique` (`token`),
  KEY `personal_access_tokens_tokenable_type_tokenable_id_index` (`tokenable_type`,`tokenable_id`)
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `personal_access_tokens`
--

LOCK TABLES `personal_access_tokens` WRITE;
/*!40000 ALTER TABLE `personal_access_tokens` DISABLE KEYS */;
INSERT INTO `personal_access_tokens` VALUES (6,'App\\Models\\User',1,'spa','b7a85498b18e5797c80a87bedacf7deb3d02795f1ef9fe4cdc9f516089c00706','[\"*\"]','2026-04-15 15:28:58',NULL,'2026-04-15 15:27:48','2026-04-15 15:28:58'),(29,'App\\Models\\User',2,'spa','026e3ed2c3ec8c8207a810e73ef82cbae03172e802bfb0fb8762d2ac6e3b80de','[\"*\"]','2026-04-27 22:39:59',NULL,'2026-04-27 21:35:07','2026-04-27 21:35:07');
/*!40000 ALTER TABLE `personal_access_tokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `potencial_clients`
--

DROP TABLE IF EXISTS `potencial_clients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `potencial_clients` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `potencial_clients`
--

LOCK TABLES `potencial_clients` WRITE;
/*!40000 ALTER TABLE `potencial_clients` DISABLE KEYS */;
INSERT INTO `potencial_clients` VALUES (2,'Banco Galicia','2026-04-16 21:56:52','2026-04-16 21:56:52');
/*!40000 ALTER TABLE `potencial_clients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `potencial_project_allocations`
--

DROP TABLE IF EXISTS `potencial_project_allocations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `potencial_project_allocations` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `potencial_project_id` bigint unsigned NOT NULL,
  `month_key` varchar(7) COLLATE utf8mb4_unicode_ci NOT NULL,
  `month_label` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `user_id` bigint unsigned DEFAULT NULL,
  `user_name` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `hours` decimal(10,2) NOT NULL DEFAULT '0.00',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `potencial_project_allocations_user_id_foreign` (`user_id`),
  KEY `potencial_alloc_project_month` (`potencial_project_id`,`month_key`),
  KEY `potencial_alloc_month` (`month_key`),
  CONSTRAINT `potencial_project_allocations_potencial_project_id_foreign` FOREIGN KEY (`potencial_project_id`) REFERENCES `potencial_projects` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `potencial_project_allocations_user_id_foreign` FOREIGN KEY (`user_id`) REFERENCES `clockify_users` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `potencial_project_allocations`
--

LOCK TABLES `potencial_project_allocations` WRITE;
/*!40000 ALTER TABLE `potencial_project_allocations` DISABLE KEYS */;
/*!40000 ALTER TABLE `potencial_project_allocations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `potencial_projects`
--

DROP TABLE IF EXISTS `potencial_projects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `potencial_projects` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `code` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `potencial_client_id` bigint unsigned NOT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `potencial_projects_potencial_client_id_foreign` (`potencial_client_id`),
  CONSTRAINT `potencial_projects_potencial_client_id_foreign` FOREIGN KEY (`potencial_client_id`) REFERENCES `potencial_clients` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `potencial_projects`
--

LOCK TABLES `potencial_projects` WRITE;
/*!40000 ALTER TABLE `potencial_projects` DISABLE KEYS */;
/*!40000 ALTER TABLE `potencial_projects` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `profiles`
--

DROP TABLE IF EXISTS `profiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `profiles` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `profiles_code_unique` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `profiles`
--

LOCK TABLES `profiles` WRITE;
/*!40000 ALTER TABLE `profiles` DISABLE KEYS */;
INSERT INTO `profiles` VALUES
(1,'Administrador','admin','Acceso total al sistema',NULL,NULL),
(2,'Soporte','soporte','Acceso total sobre módulo Configuración',NULL,NULL),
(3,'Operaciones Gerente','ops_gerente','Acceso total sobre Operación, Análisis y Reportería',NULL,NULL),
(4,'Operaciones Líderes','ops_lider','Acceso total sobre Operación, Análisis y Reportería',NULL,NULL),
(5,'Administración','administracion','Acceso total sobre Administración y Reportería',NULL,NULL);
/*!40000 ALTER TABLE `profiles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `project_intake_category_refs`
--

DROP TABLE IF EXISTS `project_intake_category_refs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `project_intake_category_refs` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `code` varchar(40) COLLATE utf8mb4_unicode_ci NOT NULL,
  `label` varchar(120) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_intake_category_refs_code_unique` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `project_intake_category_refs`
--

LOCK TABLES `project_intake_category_refs` WRITE;
/*!40000 ALTER TABLE `project_intake_category_refs` DISABLE KEYS */;
INSERT INTO `project_intake_category_refs` VALUES (9,'PRE','Pre-venta','Proyectos en etapa de preventa',1,'2026-04-27 19:35:12','2026-04-27 19:35:12'),(10,'DES','Desarrollo','Proyectos en desarrollo activo',1,'2026-04-27 19:35:12','2026-04-27 19:35:12'),(11,'SOP','Soporte','Proyectos en etapa de soporte',1,'2026-04-27 19:35:12','2026-04-27 19:35:12'),(12,'I+D','I+D','Investigaci├│n y desarrollo',1,'2026-04-27 19:35:12','2026-04-27 19:35:12'),(13,'SWF','SWFactory','Proyectos bajo modalidad SW Factory',1,'2026-04-27 19:35:12','2026-04-27 19:35:12'),(14,'DEV','DevOps','Proyectos de infraestructura y DevOps',1,'2026-04-27 19:35:12','2026-04-27 19:35:12'),(15,'STAFF','Staff Augmentation','Proyectos de staffing',1,'2026-04-27 19:35:12','2026-04-27 19:35:12'),(16,'PROXY','Proxy Int.','Proyectos de proxy internacional',1,'2026-04-27 19:35:12','2026-04-27 19:35:12');
/*!40000 ALTER TABLE `project_intake_category_refs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `project_intake_records`
--

DROP TABLE IF EXISTS `project_intake_records`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `project_intake_records` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `project_type` varchar(2) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `internal_project_number` varchar(120) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `secondary_project_number` varchar(120) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `registration_date` date DEFAULT NULL,
  `client_name` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `client_id` bigint unsigned DEFAULT NULL,
  `project_name` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `category_code` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `project_status_code` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `business_status_date` date DEFAULT NULL,
  `estimated_end_date` date DEFAULT NULL,
  `actual_end_date` date DEFAULT NULL,
  `commercial_status` varchar(120) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `leader_clockify_user_id` bigint unsigned DEFAULT NULL,
  `observations` text COLLATE utf8mb4_unicode_ci,
  `requires_clockify_creation` tinyint(1) NOT NULL DEFAULT '0',
  `clockify_record_id` bigint unsigned DEFAULT NULL,
  `created_by` bigint unsigned DEFAULT NULL,
  `updated_by` bigint unsigned DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `project_intake_records_project_type_created_at_index` (`project_type`,`created_at`),
  KEY `project_intake_records_project_status_code_index` (`project_status_code`),
  KEY `project_intake_records_category_code_index` (`category_code`),
  KEY `project_intake_records_requires_clockify_creation_index` (`requires_clockify_creation`),
  KEY `idx_intake_is_active` (`is_active`),
  KEY `fk_intake_clockify_project` (`clockify_record_id`),
  KEY `idx_intake_client_id` (`client_id`),
  KEY `idx_intake_leader` (`leader_clockify_user_id`),
  CONSTRAINT `fk_intake_category_code` FOREIGN KEY (`category_code`) REFERENCES `project_intake_category_refs` (`code`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_intake_client` FOREIGN KEY (`client_id`) REFERENCES `clockify_clients` (`id`) ON DELETE SET NULL,
  CONSTRAINT `fk_intake_clockify_project` FOREIGN KEY (`clockify_record_id`) REFERENCES `clockify_projects` (`id`) ON DELETE SET NULL,
  CONSTRAINT `fk_intake_leader` FOREIGN KEY (`leader_clockify_user_id`) REFERENCES `clockify_users` (`id`) ON DELETE SET NULL,
  CONSTRAINT `fk_intake_project_type` FOREIGN KEY (`project_type`) REFERENCES `project_intake_type_refs` (`code`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_intake_status_code` FOREIGN KEY (`project_status_code`) REFERENCES `project_intake_status_refs` (`code`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `project_intake_records`
--

LOCK TABLES `project_intake_records` WRITE;
/*!40000 ALTER TABLE `project_intake_records` DISABLE KEYS */;
INSERT INTO `project_intake_records` VALUES (3,'30','30.001','COM-2024-001','2024-03-01','Cliente Ejemplo S.A.',NULL,'Sistema de Gesti├│n Interna','DES','EN_CURSO','2024-03-01','2024-12-31',NULL,NULL,6,'Proyecto piloto de prueba',0,NULL,1,2,0,'2026-04-27 19:35:26','2026-04-27 19:40:48'),(4,'30','30.002','COM-2024-002','2024-06-15','Otra Empresa SRL',NULL,'Portal de Clientes v2','PRE','INGRESO','2024-06-15','2025-03-31',NULL,NULL,7,'En evaluaci├│n t├®cnica',0,NULL,1,1,1,'2026-04-27 19:37:12','2026-04-27 19:37:12'),(7,'30','30.003',NULL,'2024-09-01','Proyecto Cancelado SA',NULL,'Proyecto Cancelado','PRE','PERDIDO','2024-09-01','2024-12-01',NULL,NULL,NULL,'Cancelado por el cliente antes de iniciar',0,NULL,1,1,0,'2026-04-27 19:37:32','2026-04-27 19:37:32'),(8,'30','30.004','COM-2025-099','2025-04-27','Anthropic Testing Corp',NULL,'Plataforma IA Interna','DES','INGRESO','2025-04-27','2025-12-31',NULL,NULL,5,'Proyecto de prueba con alta en Clockify',1,7,2,2,1,'2026-04-27 19:43:11','2026-04-27 19:43:11'),(10,'30','30.005','COM-2025-099','2025-04-27','BDT Global',3,'Plataforma IA Interna Audit','DES','INGRESO','2025-04-27','2025-12-31',NULL,NULL,5,'Proyecto con alta en Clockify',1,8,2,2,1,'2026-04-27 20:18:45','2026-04-27 20:18:45');
/*!40000 ALTER TABLE `project_intake_records` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `project_intake_status_refs`
--

DROP TABLE IF EXISTS `project_intake_status_refs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `project_intake_status_refs` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `code` varchar(40) COLLATE utf8mb4_unicode_ci NOT NULL,
  `label` varchar(120) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_intake_status_refs_code_unique` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `project_intake_status_refs`
--

LOCK TABLES `project_intake_status_refs` WRITE;
/*!40000 ALTER TABLE `project_intake_status_refs` DISABLE KEYS */;
INSERT INTO `project_intake_status_refs` VALUES (7,'INGRESO','Ingreso','Proyecto ingresado y en evaluaci├│n',1,'2026-04-27 19:35:18','2026-04-27 19:35:18'),(8,'EN_CURSO','En curso','Proyecto aprobado y en ejecuci├│n',1,'2026-04-27 19:35:18','2026-04-27 19:35:18'),(9,'PERDIDO','Perdido','Proyecto no adjudicado o cancelado por cliente',1,'2026-04-27 19:35:18','2026-04-27 19:35:18'),(10,'CERRADO','Cerrado','Proyecto finalizado y cerrado formalmente',1,'2026-04-27 19:35:18','2026-04-27 19:35:18'),(11,'SUSPENDIDO','Suspendido','Proyecto pausado temporalmente',1,'2026-04-27 19:35:18','2026-04-27 19:35:18');
/*!40000 ALTER TABLE `project_intake_status_refs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `project_intake_type_refs`
--

DROP TABLE IF EXISTS `project_intake_type_refs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `project_intake_type_refs` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `code` varchar(2) COLLATE utf8mb4_unicode_ci NOT NULL,
  `label` varchar(120) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `internal_label` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `secondary_label` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `registration_label` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `requires_business_status_date` tinyint(1) NOT NULL DEFAULT '1',
  `requires_actual_end_date` tinyint(1) NOT NULL DEFAULT '0',
  `requires_commercial_fields` tinyint(1) NOT NULL DEFAULT '0',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_intake_type_refs_code_unique` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `project_intake_type_refs`
--

LOCK TABLES `project_intake_type_refs` WRITE;
/*!40000 ALTER TABLE `project_intake_type_refs` DISABLE KEYS */;
INSERT INTO `project_intake_type_refs` VALUES (1,'30','Desarrollo','Proyectos de desarrollo de software','Nro. Proyecto Desarrollo','Nro. Proyecto Comercial','Fecha de Alta',1,0,0,1,'2026-04-27 19:35:06','2026-04-27 19:35:06');
/*!40000 ALTER TABLE `project_intake_type_refs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `project_tracking_updates`
--

DROP TABLE IF EXISTS `project_tracking_updates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `project_tracking_updates` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `project_tracking_id` bigint unsigned NOT NULL,
  `change_end_date` date DEFAULT NULL,
  `observations` text COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `project_tracking_updates_project_tracking_id_foreign` (`project_tracking_id`),
  CONSTRAINT `project_tracking_updates_project_tracking_id_foreign` FOREIGN KEY (`project_tracking_id`) REFERENCES `project_trackings` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `project_tracking_updates`
--

LOCK TABLES `project_tracking_updates` WRITE;
/*!40000 ALTER TABLE `project_tracking_updates` DISABLE KEYS */;
/*!40000 ALTER TABLE `project_tracking_updates` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `project_trackings`
--

DROP TABLE IF EXISTS `project_trackings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `project_trackings` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `project_id` bigint unsigned NOT NULL,
  `start_date` date DEFAULT NULL,
  `planned_end_date` date DEFAULT NULL,
  `actual_end_date` date DEFAULT NULL,
  `implementation_date` date DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_trackings_project_id_unique` (`project_id`),
  CONSTRAINT `project_trackings_project_id_foreign` FOREIGN KEY (`project_id`) REFERENCES `clockify_projects` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `project_trackings`
--

LOCK TABLES `project_trackings` WRITE;
/*!40000 ALTER TABLE `project_trackings` DISABLE KEYS */;
/*!40000 ALTER TABLE `project_trackings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `user_dashboard_filters`
--

DROP TABLE IF EXISTS `user_dashboard_filters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_dashboard_filters` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `name` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `leader_id` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `month_keys` json DEFAULT NULL,
  `project_id` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `user_dashboard_filters_user_id_foreign` (`user_id`),
  CONSTRAINT `user_dashboard_filters_user_id_foreign` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `user_dashboard_filters`
--

LOCK TABLES `user_dashboard_filters` WRITE;
/*!40000 ALTER TABLE `user_dashboard_filters` DISABLE KEYS */;
/*!40000 ALTER TABLE `user_dashboard_filters` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `user_hours_summary`
--

DROP TABLE IF EXISTS `user_hours_summary`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_hours_summary` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `project_id` int unsigned DEFAULT NULL,
  `client_id` int unsigned DEFAULT NULL,
  `leader_id` bigint unsigned DEFAULT NULL,
  `month_key` varchar(7) COLLATE utf8mb4_unicode_ci NOT NULL,
  `year` int NOT NULL,
  `month` int NOT NULL,
  `duration_hours` decimal(10,3) NOT NULL DEFAULT '0.000',
  `expected_hours` decimal(10,3) NOT NULL DEFAULT '0.000',
  `notes` text COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_uhs_user_project_month` (`user_id`,`project_id`,`month_key`),
  KEY `idx_uhs_user_month` (`user_id`,`month_key`),
  KEY `idx_uhs_project_month` (`project_id`,`month_key`),
  KEY `idx_uhs_client_month` (`client_id`,`month_key`),
  KEY `idx_uhs_leader_month` (`leader_id`,`month_key`),
  KEY `idx_uhs_month_key` (`month_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `user_hours_summary`
--

LOCK TABLES `user_hours_summary` WRITE;
/*!40000 ALTER TABLE `user_hours_summary` DISABLE KEYS */;
/*!40000 ALTER TABLE `user_hours_summary` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `user_leaders`
--

DROP TABLE IF EXISTS `user_leaders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_leaders` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `leader_id` bigint unsigned NOT NULL,
  `start_date` date NOT NULL,
  `end_date` date DEFAULT NULL,
  `notes` text COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ul_user_leader_start` (`user_id`,`leader_id`,`start_date`),
  KEY `idx_ul_user` (`user_id`),
  KEY `idx_ul_leader` (`leader_id`),
  KEY `idx_ul_user_start` (`user_id`,`start_date`),
  KEY `idx_ul_leader_start` (`leader_id`,`start_date`),
  CONSTRAINT `user_leaders_leader_id_foreign` FOREIGN KEY (`leader_id`) REFERENCES `clockify_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `user_leaders_user_id_foreign` FOREIGN KEY (`user_id`) REFERENCES `clockify_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `user_leaders`
--

LOCK TABLES `user_leaders` WRITE;
/*!40000 ALTER TABLE `user_leaders` DISABLE KEYS */;
INSERT INTO `user_leaders` VALUES (3,5,6,'2026-01-01','2026-04-21','Asignaci├│n inicial','2026-04-21 00:16:09','2026-04-21 00:17:54'),(4,7,6,'2026-04-21','2026-04-21',NULL,'2026-04-21 00:16:30','2026-04-21 00:18:18'),(6,5,7,'2026-01-01',NULL,NULL,'2026-04-21 00:17:54','2026-04-21 00:17:54'),(7,6,7,'2026-01-01','2026-04-21',NULL,'2026-04-21 00:17:54','2026-04-21 00:18:18'),(8,7,5,'2026-01-01',NULL,NULL,'2026-04-21 00:18:18','2026-04-21 00:18:18'),(9,6,5,'2026-01-01',NULL,NULL,'2026-04-21 00:18:18','2026-04-21 00:18:18'),(10,8,5,'2025-01-01',NULL,'Asignaci├│n inicial','2026-04-21 17:29:19','2026-04-21 17:29:19');
/*!40000 ALTER TABLE `user_leaders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `user_monthly_capacities`
--

DROP TABLE IF EXISTS `user_monthly_capacities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_monthly_capacities` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `month_key` varchar(7) COLLATE utf8mb4_unicode_ci NOT NULL,
  `month_label` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `hours` decimal(8,2) NOT NULL DEFAULT '0.00',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_umc_user_month` (`user_id`,`month_key`),
  KEY `idx_umc_month_key` (`month_key`),
  CONSTRAINT `user_monthly_capacities_user_id_foreign` FOREIGN KEY (`user_id`) REFERENCES `clockify_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `user_monthly_capacities`
--

LOCK TABLES `user_monthly_capacities` WRITE;
/*!40000 ALTER TABLE `user_monthly_capacities` DISABLE KEYS */;
INSERT INTO `user_monthly_capacities` VALUES (1,5,'2026-01','Enero 2026',120.00,'2026-04-21 01:44:55','2026-04-21 01:45:16'),(3,5,'2026-03','Marzo 2026',168.00,'2026-04-21 01:44:55','2026-04-21 01:44:55'),(4,5,'2026-04','Abril 2026',176.00,'2026-04-21 01:45:29','2026-04-21 01:45:29'),(5,8,'2025-05','Mayo 2025',140.00,'2026-04-21 17:44:03','2026-04-21 17:44:03'),(6,8,'2025-06','Junio 2025',160.00,'2026-04-21 17:44:03','2026-04-21 17:44:03'),(7,8,'2025-07','Julio 2025',120.00,'2026-04-21 17:44:03','2026-04-21 17:44:03');
/*!40000 ALTER TABLE `user_monthly_capacities` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `user_monthly_status`
--

DROP TABLE IF EXISTS `user_monthly_status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_monthly_status` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `month_key` varchar(7) COLLATE utf8mb4_unicode_ci NOT NULL,
  `year` int NOT NULL,
  `month` int NOT NULL,
  `status` enum('activo','inactivo','vacaciones','licencia','baja') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'activo',
  `status_start_date` date DEFAULT NULL,
  `status_end_date` date DEFAULT NULL,
  `days_in_status` int NOT NULL DEFAULT '0',
  `notes` text COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ums_user_month` (`user_id`,`month_key`),
  KEY `idx_ums_user_month` (`user_id`,`month_key`),
  KEY `idx_ums_user_year_month` (`user_id`,`year`,`month`),
  KEY `idx_ums_month_key` (`month_key`),
  KEY `idx_ums_status` (`status`),
  CONSTRAINT `user_monthly_status_user_id_foreign` FOREIGN KEY (`user_id`) REFERENCES `clockify_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `user_monthly_status`
--

LOCK TABLES `user_monthly_status` WRITE;
/*!40000 ALTER TABLE `user_monthly_status` DISABLE KEYS */;
/*!40000 ALTER TABLE `user_monthly_status` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `user_vacation_periods`
--

DROP TABLE IF EXISTS `user_vacation_periods`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_vacation_periods` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `date_from` date NOT NULL,
  `date_to` date NOT NULL,
  `total_days` int unsigned NOT NULL DEFAULT '0',
  `notes` text COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `user_vacation_periods_user_id_date_from_index` (`user_id`,`date_from`),
  CONSTRAINT `user_vacation_periods_user_id_foreign` FOREIGN KEY (`user_id`) REFERENCES `clockify_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `user_vacation_periods`
--

LOCK TABLES `user_vacation_periods` WRITE;
/*!40000 ALTER TABLE `user_vacation_periods` DISABLE KEYS */;
INSERT INTO `user_vacation_periods` VALUES (1,5,'2024-05-01','2024-05-15',15,'Vacaciones de invierno','2026-04-15 15:26:35','2026-04-15 15:26:35'),(2,7,'2026-05-01','2026-05-10',10,'Vacaciones de mayo','2026-04-21 02:08:07','2026-04-21 02:08:07'),(4,6,'2026-06-01','2026-06-05',5,NULL,'2026-04-21 02:08:33','2026-04-21 02:08:33'),(5,8,'2025-07-01','2025-07-15',15,'Vacaciones de verano','2026-04-21 18:58:38','2026-04-21 18:58:38');
/*!40000 ALTER TABLE `user_vacation_periods` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `email` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `email_verified_at` timestamp NULL DEFAULT NULL,
  `password` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `profile_id` bigint unsigned DEFAULT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  `remember_token` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `users_email_unique` (`email`),
  KEY `users_profile_id_foreign` (`profile_id`),
  CONSTRAINT `users_profile_id_foreign` FOREIGN KEY (`profile_id`) REFERENCES `profiles` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES
(1,'Admin','admin@evm.com',NULL,'$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy',1,1,NULL,NULL,NULL),
(2,'Daniel','daniel@test.com',NULL,'$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy',1,1,NULL,NULL,NULL),
(3,'Diego','diego@test.com',NULL,'$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy',1,1,NULL,NULL,NULL),
(4,'Lucia','lucia@test.com',NULL,'$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy',1,1,NULL,NULL,NULL),
(5,'Santiago','santi@test.com',NULL,'$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy',1,1,NULL,NULL,NULL),
(6,'Christian','chris@test.com',NULL,'$2y$12$rPlsryMWohVrw475KUzlDOhColDT7qLIPq5QDS6hIUhe7Bp/jCioy',1,1,NULL,NULL,NULL);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Estructura de la tabla `working_days_calendar`
--

DROP TABLE IF EXISTS `working_days_calendar`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `working_days_calendar` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `month_key` varchar(7) COLLATE utf8mb4_unicode_ci NOT NULL,
  `month_label` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `year` int NOT NULL,
  `month` int NOT NULL,
  `total_days` int NOT NULL,
  `working_days` int NOT NULL,
  `hours_month` decimal(10,2) NOT NULL DEFAULT '0.00',
  `holiday_days` int NOT NULL DEFAULT '0',
  `holidays_list` text COLLATE utf8mb4_unicode_ci,
  `notes` text COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_wdc_year_month` (`year`,`month`),
  UNIQUE KEY `working_days_calendar_month_key_unique` (`month_key`),
  KEY `idx_wdc_month_key` (`month_key`),
  KEY `idx_wdc_year_month` (`year`,`month`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Datos para la tabla `working_days_calendar`
--

LOCK TABLES `working_days_calendar` WRITE;
/*!40000 ALTER TABLE `working_days_calendar` DISABLE KEYS */;
-- Columnas: id, month_key, month_label, year, month, total_days, working_days, hours_month, holiday_days, holidays_list, notes, created_at, updated_at
-- Feriados nacionales Argentina 2026 (inamovibles + móviles calculados)
INSERT INTO `working_days_calendar` VALUES
(1, '2026-01','Enero 2026',     2026,1, 31,21,168.00,1, '[\"2026-01-01\"]',                       NULL,NULL,NULL),
(2, '2026-02','Febrero 2026',   2026,2, 28,18,144.00,2, '[\"2026-02-16\",\"2026-02-17\"]',         'Carnaval',NULL,NULL),
(3, '2026-03','Marzo 2026',     2026,3, 31,21,168.00,1, '[\"2026-03-24\"]',                       NULL,NULL,NULL),
(4, '2026-04','Abril 2026',     2026,4, 30,20,160.00,2, '[\"2026-04-02\",\"2026-04-03\"]',         'Semana Santa',NULL,NULL),
(5, '2026-05','Mayo 2026',      2026,5, 31,19,152.00,2, '[\"2026-05-01\",\"2026-05-25\"]',         NULL,NULL,NULL),
(6, '2026-06','Junio 2026',     2026,6, 30,21,168.00,1, '[\"2026-06-17\"]',                       'Jun 20 cae sábado',NULL,NULL),
(7, '2026-07','Julio 2026',     2026,7, 31,22,176.00,1, '[\"2026-07-09\"]',                       NULL,NULL,NULL),
(8, '2026-08','Agosto 2026',    2026,8, 31,20,160.00,1, '[\"2026-08-17\"]',                       NULL,NULL,NULL),
(9, '2026-09','Septiembre 2026',2026,9, 30,22,176.00,0, NULL,                                     NULL,NULL,NULL),
(10,'2026-10','Octubre 2026',   2026,10,31,21,168.00,1, '[\"2026-10-12\"]',                       NULL,NULL,NULL),
(11,'2026-11','Noviembre 2026', 2026,11,30,20,160.00,1, '[\"2026-11-20\"]',                       NULL,NULL,NULL),
(12,'2026-12','Diciembre 2026', 2026,12,31,21,168.00,2, '[\"2026-12-08\",\"2026-12-25\"]',         NULL,NULL,NULL);
/*!40000 ALTER TABLE `working_days_calendar` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Exportación completada el 2026-04-28 00:03:09
