CREATE TABLE IF NOT EXISTS "plc_logs" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "plc_id" INTEGER NOT NULL,
  "message" TEXT NOT NULL,
  "created_at" TEXT NOT NULL
);