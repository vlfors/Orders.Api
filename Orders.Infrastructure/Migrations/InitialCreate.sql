CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE orders (
    "Id" uuid NOT NULL,
    "CustomerName" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL,
    "TotalAmount" numeric NOT NULL,
    CONSTRAINT "PK_orders" PRIMARY KEY ("Id")
);

CREATE TABLE order_items (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "ProductName" character varying(200) NOT NULL,
    "Quantity" integer NOT NULL,
    "Price" numeric NOT NULL,
    CONSTRAINT "PK_order_items" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_order_items_orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES orders ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_order_items_OrderId" ON order_items ("OrderId");

CREATE INDEX "IX_orders_Status_CreatedAt" ON orders ("Status", "CreatedAt");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260906082359_InitialCreate', '10.0.11');

COMMIT;

