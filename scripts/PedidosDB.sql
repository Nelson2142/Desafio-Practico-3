/* =============================================================================
   DES104 - Desarrollo de Software Empresarial
   Desafío práctico 3 - Sistema de Pedidos (ASP.NET Core Web API, N capas)
   Autor: Nelson Steven Rivas Tobar - RT221663

   Script de creación de la base de datos PedidosDB.

   NOTA: no es necesario ejecutarlo. La API aplica la migración de EF Core
   (CreacionInicial) automáticamente al arrancar. Se incluye como entregable y
   para crear la base manualmente desde SSMS / Azure Data Studio si se desea.
   El esquema es idéntico al que genera la migración, y registra la migración
   en __EFMigrationsHistory para que EF Core no intente crearla de nuevo.
   ============================================================================= */

IF DB_ID(N'PedidosDB') IS NULL
    CREATE DATABASE PedidosDB;
GO

USE PedidosDB;
GO

/* ----------------------------- Historial EF Core ----------------------------- */
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId]    nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261015060000_CreacionInicial')
BEGIN

    /* ------------------------------- Clientes -------------------------------- */
    CREATE TABLE [Clientes] (
        [Id]       int           NOT NULL IDENTITY(1,1),
        [Nombre]   nvarchar(50)  NOT NULL,
        [Apellido] nvarchar(50)  NOT NULL,
        [Email]    nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );

    /* ------------------------------- Productos ------------------------------- */
    CREATE TABLE [Productos] (
        [Id]          int           NOT NULL IDENTITY(1,1),
        [Nombre]      nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(500) NOT NULL,
        [Precio]      decimal(18,2) NOT NULL,
        [Stock]       int           NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id])
    );

    /* -------------------------------- Pedidos -------------------------------- */
    CREATE TABLE [Pedidos] (
        [Id]             int           NOT NULL IDENTITY(1,1),
        [FechaPedido]    datetime2     NOT NULL,
        [Cantidad]       int           NOT NULL,
        [ClienteId]      int           NOT NULL,
        [ProductoId]     int           NOT NULL,
        [PrecioUnitario] decimal(18,2) NOT NULL,
        [Total]          decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Pedidos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pedidos_Clientes_ClienteId]   FOREIGN KEY ([ClienteId])  REFERENCES [Clientes]  ([Id]),
        CONSTRAINT [FK_Pedidos_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id])
    );

    CREATE UNIQUE INDEX [IX_Clientes_Email]     ON [Clientes] ([Email]);
    CREATE INDEX        [IX_Pedidos_ClienteId]  ON [Pedidos]  ([ClienteId]);
    CREATE INDEX        [IX_Pedidos_ProductoId] ON [Pedidos]  ([ProductoId]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261015060000_CreacionInicial', N'8.0.10');
END;
GO

/* ---------------------------- Datos de ejemplo ------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [Clientes]) AND NOT EXISTS (SELECT 1 FROM [Productos])
BEGIN
    INSERT INTO [Clientes] ([Nombre], [Apellido], [Email]) VALUES
        (N'Ana',    N'Martínez',  N'ana.martinez@correo.com'),
        (N'Carlos', N'Hernández', N'carlos.hernandez@correo.com'),
        (N'Sofía',  N'López',     N'sofia.lopez@correo.com');

    INSERT INTO [Productos] ([Nombre], [Descripcion], [Precio], [Stock]) VALUES
        (N'Laptop Lenovo IdeaPad 3', N'Laptop 15.6 pulgadas, 16 GB RAM, 512 GB SSD', 649.99, 14),
        (N'Mouse inalámbrico',       N'Mouse óptico inalámbrico 2.4 GHz',             18.50, 77),
        (N'Teclado mecánico',        N'Teclado mecánico retroiluminado en español',   54.75, 30),
        (N'Monitor 24 pulgadas',     N'Monitor Full HD IPS de 24 pulgadas',           139.00, 10);

    -- El stock de arriba ya descuenta las unidades de estos pedidos.
    INSERT INTO [Pedidos] ([FechaPedido], [Cantidad], [ClienteId], [ProductoId], [PrecioUnitario], [Total]) VALUES
        (DATEADD(DAY, -3, SYSDATETIME()), 1, 1, 1, 649.99, 649.99),
        (DATEADD(DAY, -1, SYSDATETIME()), 3, 2, 2, 18.50,  55.50);
END;
GO

/* ------------------------------ Verificación --------------------------------- */
SELECT p.Id, p.FechaPedido, c.Nombre + N' ' + c.Apellido AS Cliente,
       pr.Nombre AS Producto, p.Cantidad, p.PrecioUnitario, p.Total
FROM [Pedidos] p
JOIN [Clientes]  c  ON c.Id  = p.ClienteId
JOIN [Productos] pr ON pr.Id = p.ProductoId
ORDER BY p.FechaPedido DESC;
GO
