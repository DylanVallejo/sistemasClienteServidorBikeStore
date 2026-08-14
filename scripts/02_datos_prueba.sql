-- =====================================================================
--  BikeStore - Datos de prueba (INSERT)
--  Ejecutar DESPUES de 01_crear_bd.sql
-- =====================================================================

USE BikeStoreDB;
GO

-- Categorias (las 5 sugeridas por el proyecto).
INSERT INTO Categoria (Nombre, Descripcion, Activo) VALUES
('Montana',    'Bicicletas para terreno montanoso y todo terreno', 1),
('Ruta',       'Bicicletas ligeras para asfalto y velocidad',      1),
('BMX',        'Bicicletas para acrobacias y saltos',              1),
('Electricas', 'Bicicletas con motor electrico de asistencia',     1),
('Infantiles', 'Bicicletas para ninos',                            1);
GO

-- Bicicletas de ejemplo (incluye algunas con stock bajo/agotado para pruebas).
INSERT INTO Bicicleta (IdCategoria, Marca, Modelo, Precio, Stock, Estado) VALUES
(1, 'Trek',      'Marlin 5',        650.00, 12, 'Disponible'),
(1, 'Specialized','Rockhopper',     780.00,  4, 'Disponible'),
(2, 'Giant',     'Contend 3',       920.00,  8, 'Disponible'),
(2, 'Scott',     'Speedster 40',   1100.00,  0, 'Agotado'),
(3, 'GT',        'Performer 26',    430.00, 15, 'Disponible'),
(4, 'Cannondale','Quick Neo',      2200.00,  3, 'Disponible'),
(5, 'Trek',      'Precaliber 16',   280.00, 20, 'Disponible');
GO

-- Clientes de ejemplo.
INSERT INTO Cliente (Cedula, Nombres, Apellidos, Telefono, Correo) VALUES
('0102030405', 'Juan',   'Perez Lopez',      '0991112233', 'juan.perez@correo.com'),
('0203040506', 'Maria',  'Gomez Andrade',    '0982223344', 'maria.gomez@correo.com'),
('0304050607', 'Carlos', 'Torres Vega',      '0973334455', 'carlos.torres@correo.com');
GO

PRINT 'Datos de prueba insertados correctamente.';
GO
