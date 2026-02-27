-- MySQL schema for warehouse project
-- Creates database + core tables based on current UML/UseCase diagrams.

CREATE DATABASE IF NOT EXISTS warehouse_db
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE warehouse_db;

-- Drop order (children first) for idempotent re-run in dev.
DROP TABLE IF EXISTS refresh_tokens;
DROP TABLE IF EXISTS stock_movements;
DROP TABLE IF EXISTS issue_request_items;
DROP TABLE IF EXISTS issue_requests;
DROP TABLE IF EXISTS stock_balances;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS warehouses;
DROP TABLE IF EXISTS employees;
DROP TABLE IF EXISTS user_roles;
DROP TABLE IF EXISTS roles;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS departments;

CREATE TABLE departments (
  id INT NOT NULL AUTO_INCREMENT,
  name VARCHAR(150) NOT NULL,
  code VARCHAR(30) NOT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_departments_code (code),
  UNIQUE KEY uq_departments_name (name)
) ENGINE=InnoDB;

CREATE TABLE users (
  id CHAR(36) NOT NULL,
  username VARCHAR(80) NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  email VARCHAR(150) NOT NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  last_login_at DATETIME NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_users_username (username),
  UNIQUE KEY uq_users_email (email)
) ENGINE=InnoDB;

CREATE TABLE roles (
  id INT NOT NULL AUTO_INCREMENT,
  name VARCHAR(50) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_roles_name (name)
) ENGINE=InnoDB;

CREATE TABLE user_roles (
  user_id CHAR(36) NOT NULL,
  role_id INT NOT NULL,
  assigned_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (user_id, role_id),
  CONSTRAINT fk_user_roles_user
    FOREIGN KEY (user_id) REFERENCES users (id)
    ON DELETE CASCADE,
  CONSTRAINT fk_user_roles_role
    FOREIGN KEY (role_id) REFERENCES roles (id)
    ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE employees (
  id CHAR(36) NOT NULL,
  user_id CHAR(36) NOT NULL,
  department_id INT NOT NULL,
  full_name VARCHAR(200) NOT NULL,
  position VARCHAR(120) NOT NULL,
  phone VARCHAR(30) NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_employees_user_id (user_id),
  KEY ix_employees_department_id (department_id),
  CONSTRAINT fk_employees_user
    FOREIGN KEY (user_id) REFERENCES users (id)
    ON DELETE CASCADE,
  CONSTRAINT fk_employees_department
    FOREIGN KEY (department_id) REFERENCES departments (id)
    ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE warehouses (
  id INT NOT NULL AUTO_INCREMENT,
  name VARCHAR(150) NOT NULL,
  address VARCHAR(255) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_warehouses_name (name)
) ENGINE=InnoDB;

CREATE TABLE products (
  id INT NOT NULL AUTO_INCREMENT,
  sku VARCHAR(60) NOT NULL,
  name VARCHAR(200) NOT NULL,
  unit VARCHAR(20) NOT NULL,
  description VARCHAR(500) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_products_sku (sku),
  KEY ix_products_name (name)
) ENGINE=InnoDB;

CREATE TABLE stock_balances (
  id BIGINT NOT NULL AUTO_INCREMENT,
  warehouse_id INT NOT NULL,
  product_id INT NOT NULL,
  quantity DECIMAL(18,3) NOT NULL DEFAULT 0,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
    ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_stock_balances_warehouse_product (warehouse_id, product_id),
  KEY ix_stock_balances_product (product_id),
  CONSTRAINT fk_stock_balances_warehouse
    FOREIGN KEY (warehouse_id) REFERENCES warehouses (id)
    ON DELETE RESTRICT,
  CONSTRAINT fk_stock_balances_product
    FOREIGN KEY (product_id) REFERENCES products (id)
    ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE issue_requests (
  id BIGINT NOT NULL AUTO_INCREMENT,
  requester_id CHAR(36) NOT NULL,
  department_id INT NOT NULL,
  status ENUM(
    'Draft', 'Submitted', 'Approved', 'Rejected', 'InProgress', 'PartiallyIssued', 'Issued', 'Cancelled'
  ) NOT NULL DEFAULT 'Draft',
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  approved_at DATETIME NULL,
  approved_by_id CHAR(36) NULL,
  comment VARCHAR(500) NULL,
  PRIMARY KEY (id),
  KEY ix_issue_requests_requester (requester_id),
  KEY ix_issue_requests_department (department_id),
  KEY ix_issue_requests_status (status),
  KEY ix_issue_requests_approved_by (approved_by_id),
  CONSTRAINT fk_issue_requests_requester
    FOREIGN KEY (requester_id) REFERENCES users (id)
    ON DELETE RESTRICT,
  CONSTRAINT fk_issue_requests_department
    FOREIGN KEY (department_id) REFERENCES departments (id)
    ON DELETE RESTRICT,
  CONSTRAINT fk_issue_requests_approved_by
    FOREIGN KEY (approved_by_id) REFERENCES users (id)
    ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE issue_request_items (
  id BIGINT NOT NULL AUTO_INCREMENT,
  request_id BIGINT NOT NULL,
  product_id INT NOT NULL,
  requested_qty DECIMAL(18,3) NOT NULL,
  issued_qty DECIMAL(18,3) NOT NULL DEFAULT 0,
  PRIMARY KEY (id),
  UNIQUE KEY uq_issue_items_request_product (request_id, product_id),
  KEY ix_issue_items_product (product_id),
  CONSTRAINT fk_issue_items_request
    FOREIGN KEY (request_id) REFERENCES issue_requests (id)
    ON DELETE CASCADE,
  CONSTRAINT fk_issue_items_product
    FOREIGN KEY (product_id) REFERENCES products (id)
    ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE stock_movements (
  id BIGINT NOT NULL AUTO_INCREMENT,
  warehouse_id INT NOT NULL,
  product_id INT NOT NULL,
  type ENUM('Receipt', 'WriteOff', 'IssueByRequest', 'TransferIn', 'TransferOut', 'Adjustment') NOT NULL,
  quantity DECIMAL(18,3) NOT NULL,
  occurred_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  document_number VARCHAR(80) NULL,
  comment VARCHAR(500) NULL,
  performed_by_id CHAR(36) NULL,
  request_id BIGINT NULL,
  PRIMARY KEY (id),
  KEY ix_stock_movements_product_warehouse_time (product_id, warehouse_id, occurred_at),
  KEY ix_stock_movements_type (type),
  KEY ix_stock_movements_request (request_id),
  CONSTRAINT fk_stock_movements_warehouse
    FOREIGN KEY (warehouse_id) REFERENCES warehouses (id)
    ON DELETE RESTRICT,
  CONSTRAINT fk_stock_movements_product
    FOREIGN KEY (product_id) REFERENCES products (id)
    ON DELETE RESTRICT,
  CONSTRAINT fk_stock_movements_user
    FOREIGN KEY (performed_by_id) REFERENCES users (id)
    ON DELETE SET NULL,
  CONSTRAINT fk_stock_movements_request
    FOREIGN KEY (request_id) REFERENCES issue_requests (id)
    ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE refresh_tokens (
  id BIGINT NOT NULL AUTO_INCREMENT,
  user_id CHAR(36) NOT NULL,
  token_hash VARCHAR(255) NOT NULL,
  expires_at DATETIME NOT NULL,
  revoked_at DATETIME NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_refresh_tokens_hash (token_hash),
  KEY ix_refresh_tokens_user (user_id),
  CONSTRAINT fk_refresh_tokens_user
    FOREIGN KEY (user_id) REFERENCES users (id)
    ON DELETE CASCADE
) ENGINE=InnoDB;

-- Minimal reference data for roles
INSERT INTO roles (name) VALUES
  ('Admin'),
  ('Storekeeper'),
  ('DepartmentHead')
ON DUPLICATE KEY UPDATE name = VALUES(name);
