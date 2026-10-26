IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Categorias] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(50) NOT NULL,
    [Descripcion] nvarchar(200) NOT NULL,
    [Activo] bit NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Usuarios] (
    [Id] int NOT NULL IDENTITY,
    [NombreCompleto] nvarchar(100) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Rol] nvarchar(20) NOT NULL,
    [Activo] bit NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Productos] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Descripcion] nvarchar(200) NOT NULL,
    [CategoriaId] int NOT NULL,
    [PrecioUnitario] decimal(18,2) NOT NULL,
    [StockMinimo] int NOT NULL,
    [CodigoSku] nvarchar(50) NOT NULL,
    [Activo] bit NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Productos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Lotes] (
    [Id] int NOT NULL IDENTITY,
    [CodigoLote] nvarchar(50) NOT NULL,
    [ProductoId] int NOT NULL,
    [CantidadInicial] int NOT NULL,
    [CantidadDisponible] int NOT NULL,
    [PrecioCompra] decimal(18,2) NOT NULL,
    [FechaIngreso] datetime2 NOT NULL,
    [FechaVencimiento] datetime2 NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Lotes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Lotes_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Movimientos] (
    [Id] int NOT NULL IDENTITY,
    [Tipo] int NOT NULL,
    [LoteId] int NOT NULL,
    [ProductoId] int NOT NULL,
    [Cantidad] int NOT NULL,
    [PrecioUnitario] decimal(18,2) NOT NULL,
    [Fecha] datetime2 NOT NULL,
    [Observaciones] nvarchar(200) NOT NULL,
    [UsuarioResponsable] nvarchar(150) NOT NULL,
    CONSTRAINT [PK_Movimientos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Movimientos_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Movimientos_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [IX_Categorias_Nombre] ON [Categorias] ([Nombre]);
GO

CREATE UNIQUE INDEX [IX_Lotes_CodigoLote] ON [Lotes] ([CodigoLote]);
GO

CREATE INDEX [IX_Lotes_ProductoId_FechaIngreso] ON [Lotes] ([ProductoId], [FechaIngreso]);
GO

CREATE INDEX [IX_Movimientos_LoteId] ON [Movimientos] ([LoteId]);
GO

CREATE INDEX [IX_Movimientos_ProductoId_Fecha] ON [Movimientos] ([ProductoId], [Fecha]);
GO

CREATE INDEX [IX_Productos_CategoriaId] ON [Productos] ([CategoriaId]);
GO

CREATE UNIQUE INDEX [IX_Productos_CodigoSku] ON [Productos] ([CodigoSku]);
GO

CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261026053003_InitialCreate', N'8.0.11');
GO

COMMIT;
GO

