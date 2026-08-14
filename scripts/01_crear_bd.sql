-- =====================================================================
--  BikeStore - Script de creacion de la base de datos
--  Motor: SQL Server
--  Contenido: CREATE DATABASE, tablas, llaves primarias y foraneas.
-- =====================================================================

-- Crear la base de datos si no existe.
IF DB_ID('BikeStoreDB') IS NULL
    CREATE DATABASE BikeStoreDB;
GO

USE BikeStoreDB;
GO

-- Eliminar tablas si ya existen (para poder re-ejecutar el script).
-- Se borran en orden inverso a las dependencias (FK).
IF OBJECT_ID('DetalleVenta', 'U') IS NOT NULL DROP TABLE DetalleVenta;
IF OBJECT_ID('Venta', 'U')        IS NOT NULL DROP TABLE Venta;
IF OBJECT_ID('Bicicleta', 'U')    IS NOT NULL DROP TABLE Bicicleta;
IF OBJECT_ID('Cliente', 'U')      IS NOT NULL DROP TABLE Cliente;
IF OBJECT_ID('Categoria', 'U')    IS NOT NULL DROP TABLE Categoria;
GO

-- ---------------------------------------------------------------------
-- Tabla: Categoria
-- ---------------------------------------------------------------------
CREATE TABLE Categoria (
    IdCategoria  INT IDENTITY(1,1) NOT NULL,
    Nombre       NVARCHAR(50)  NOT NULL,
    Descripcion  NVARCHAR(200) NULL,
    Activo       BIT NOT NULL DEFAULT 1,
    CONSTRAINT PK_Categoria PRIMARY KEY (IdCategoria),
    CONSTRAINT UQ_Categoria_Nombre UNIQUE (Nombre)
);
GO

-- ---------------------------------------------------------------------
-- Tabla: Bicicleta
-- ---------------------------------------------------------------------
CREATE TABLE Bicicleta (
    IdBicicleta  INT IDENTITY(1,1) NOT NULL,
    IdCategoria  INT NOT NULL,
    Marca        NVARCHAR(50)  NOT NULL,
    Modelo       NVARCHAR(80)  NOT NULL,
    Precio       DECIMAL(10,2) NOT NULL,
    Stock        INT NOT NULL DEFAULT 0,
    Estado       NVARCHAR(20)  NOT NULL DEFAULT 'Disponible',
    CONSTRAINT PK_Bicicleta PRIMARY KEY (IdBicicleta),
    CONSTRAINT FK_Bicicleta_Categoria FOREIGN KEY (IdCategoria) REFERENCES Categoria(IdCategoria),
    CONSTRAINT CK_Bicicleta_Precio CHECK (Precio >= 0),
    CONSTRAINT CK_Bicicleta_Stock  CHECK (Stock >= 0)
);
GO

-- ---------------------------------------------------------------------
-- Tabla: Cliente
-- ---------------------------------------------------------------------
CREATE TABLE Cliente (
    IdCliente  INT IDENTITY(1,1) NOT NULL,
    Cedula     NVARCHAR(15) NOT NULL,
    Nombres    NVARCHAR(80) NOT NULL,
    Apellidos  NVARCHAR(80) NOT NULL,
    Telefono   NVARCHAR(15) NULL,
    Correo     NVARCHAR(100) NULL,
    CONSTRAINT PK_Cliente PRIMARY KEY (IdCliente),
    CONSTRAINT UQ_Cliente_Cedula UNIQUE (Cedula)
);
GO

-- ---------------------------------------------------------------------
-- Tabla: Venta (cabecera)
-- ---------------------------------------------------------------------
CREATE TABLE Venta (
    IdVenta    INT IDENTITY(1,1) NOT NULL,
    Fecha      DATETIME NOT NULL DEFAULT GETDATE(),
    IdCliente  INT NOT NULL,
    Subtotal   DECIMAL(10,2) NOT NULL DEFAULT 0,
    Iva        DECIMAL(10,2) NOT NULL DEFAULT 0,
    Total      DECIMAL(10,2) NOT NULL DEFAULT 0,
    CONSTRAINT PK_Venta PRIMARY KEY (IdVenta),
    CONSTRAINT FK_Venta_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente)
);
GO

-- ---------------------------------------------------------------------
-- Tabla: DetalleVenta (lineas de la venta)
-- ---------------------------------------------------------------------
CREATE TABLE DetalleVenta (
    IdDetalle    INT IDENTITY(1,1) NOT NULL,
    IdVenta      INT NOT NULL,
    IdBicicleta  INT NOT NULL,
    Cantidad     INT NOT NULL,
    Precio       DECIMAL(10,2) NOT NULL,
    Subtotal     DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_DetalleVenta PRIMARY KEY (IdDetalle),
    CONSTRAINT FK_Detalle_Venta     FOREIGN KEY (IdVenta) REFERENCES Venta(IdVenta),
    CONSTRAINT FK_Detalle_Bicicleta FOREIGN KEY (IdBicicleta) REFERENCES Bicicleta(IdBicicleta),
    CONSTRAINT CK_Detalle_Cantidad  CHECK (Cantidad > 0)
);
GO

PRINT 'Base de datos BikeStoreDB y tablas creadas correctamente.';
GO
