/*
 Navicat Premium Dump SQL

 Source Server         : etermaiweb
 Source Server Type    : PostgreSQL
 Source Server Version : 160001 (160001)
 Source Host           : 47.111.119.238:5432
 Source Catalog        : etermaidb
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 160001 (160001)
 File Encoding         : 65001

 Date: 03/07/2025 16:44:09
*/


-- ----------------------------
-- Table structure for muai34
-- ----------------------------
DROP TABLE IF EXISTS "public"."muai34";
CREATE TABLE "public"."muai34" (
  "id" int4 NOT NULL,
  "dep" varchar(255) COLLATE "pg_catalog"."default",
  "arr" varchar(255) COLLATE "pg_catalog"."default",
  "carrier" varchar(255) COLLATE "pg_catalog"."default",
  "flightno" varchar(255) COLLATE "pg_catalog"."default",
  "cabins" varchar(255) COLLATE "pg_catalog"."default",
  "extraOpenCabin" varchar(255) COLLATE "pg_catalog"."default",
  "price" numeric(10,0),
  "rebate" numeric(10,2),
  "retention" numeric(10,0),
  "StartFlightDate" varchar(255) COLLATE "pg_catalog"."default",
  "EndFlightDate" varchar(255) COLLATE "pg_catalog"."default",
  "AheadDays" int4,
  "orgWeekDays" varchar(255) COLLATE "pg_catalog"."default",
  "orgExceptDateRanges" varchar(255) COLLATE "pg_catalog"."default",
  "orgFlightNosLimit" varchar(255) COLLATE "pg_catalog"."default",
  "createTime" timestamp(6)
)
;

-- ----------------------------
-- Primary Key structure for table muai34
-- ----------------------------
ALTER TABLE "public"."muai34" ADD CONSTRAINT "muai34_pkey" PRIMARY KEY ("id");
