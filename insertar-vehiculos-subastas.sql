-- Script para insertar vehículos con subastas activas e imágenes
-- Ejecutar en la base de datos Subastas

USE Subastas;
GO

-- Insertar vehículos
DECLARE @IdVehiculo1 INT, @IdVehiculo2 INT, @IdVehiculo3 INT, @IdVehiculo4 INT, @IdVehiculo5 INT;

-- Vehículo 1: BMW X5
INSERT INTO Vehiculo (Marca, Modelo, Matricula, Color, Anio, NumeroPuertas, Kilometraje, Potencia, TipoMotor, TipoCarroceria, Descripcion, FechaMatriculacion, FechaCreacion, Estado)
VALUES ('BMW', 'X5 xDrive40i', '7892KLM', 'Negro', 2020, 5, 45000, 340, 'Gasolina', 'Todoterreno', 
        'BMW X5 en excelente estado, equipamiento completo, navegador, asientos de cuero, sistema de sonido premium.', 
        '2020-05-15', CAST(GETDATE() AS DATE), 'En Subasta');
SET @IdVehiculo1 = SCOPE_IDENTITY();

-- Vehículo 2: Mercedes-Benz Clase C
INSERT INTO Vehiculo (Marca, Modelo, Matricula, Color, Anio, NumeroPuertas, Kilometraje, Potencia, TipoMotor, TipoCarroceria, Descripcion, FechaMatriculacion, FechaCreacion, Estado)
VALUES ('Mercedes-Benz', 'Clase C 220d', '5634PMN', 'Plata', 2019, 4, 62000, 194, 'Gasoil', 'Berlina', 
        'Mercedes Clase C diésel, muy económico, ideal para ejecutivos. Revisiones al día en concesionario oficial.', 
        '2019-03-20', CAST(GETDATE() AS DATE), 'En Subasta');
SET @IdVehiculo2 = SCOPE_IDENTITY();

-- Vehículo 3: Audi A4 Avant
INSERT INTO Vehiculo (Marca, Modelo, Matricula, Color, Anio, NumeroPuertas, Kilometraje, Potencia, TipoMotor, TipoCarroceria, Descripcion, FechaMatriculacion, FechaCreacion, Estado)
VALUES ('Audi', 'A4 Avant 2.0 TDI', '9123FGH', 'Azul', 2021, 5, 28000, 150, 'Gasoil', 'Familiar', 
        'Audi A4 Avant como nuevo, acabado S-Line, llantas de 18 pulgadas, techo panorámico. Perfecto para familias.', 
        '2021-07-10', CAST(GETDATE() AS DATE), 'En Subasta');
SET @IdVehiculo3 = SCOPE_IDENTITY();

-- Vehículo 4: Tesla Model 3
INSERT INTO Vehiculo (Marca, Modelo, Matricula, Color, Anio, NumeroPuertas, Kilometraje, Potencia, TipoMotor, TipoCarroceria, Descripcion, FechaMatriculacion, FechaCreacion, Estado)
VALUES ('Tesla', 'Model 3 Long Range', '3567TML', 'Blanco', 2022, 4, 18000, 450, 'Eléctrica', 'Berlina', 
        'Tesla Model 3 eléctrico, autonomía de 580km, piloto automático mejorado, cargador rápido incluido.', 
        '2022-02-28', CAST(GETDATE() AS DATE), 'En Subasta');
SET @IdVehiculo4 = SCOPE_IDENTITY();

-- Vehículo 5: Volkswagen Tiguan
INSERT INTO Vehiculo (Marca, Modelo, Matricula, Color, Anio, NumeroPuertas, Kilometraje, Potencia, TipoMotor, TipoCarroceria, Descripcion, FechaMatriculacion, FechaCreacion, Estado)
VALUES ('Volkswagen', 'Tiguan 2.0 TDI', '8765BWC', 'Gris', 2020, 5, 52000, 150, 'Gasoil', 'Todoterreno', 
        'VW Tiguan con tracción 4x4, perfecto para viajes, amplio maletero, sensores de aparcamiento y cámara trasera.', 
        '2020-09-12', CAST(GETDATE() AS DATE), 'En Subasta');
SET @IdVehiculo5 = SCOPE_IDENTITY();

-- Insertar subastas activas (fechas de hoy a 7 días)
DECLARE @IdSubasta1 INT, @IdSubasta2 INT, @IdSubasta3 INT, @IdSubasta4 INT, @IdSubasta5 INT;

-- Subasta BMW X5
INSERT INTO Subasta (IdVehiculo, FechaInicio, FechaFin, PrecioInicial, IncrementoMinimo, PrecioActual, Estado)
VALUES (@IdVehiculo1, GETDATE(), DATEADD(day, 7, GETDATE()), 35000.00, 500.00, 35000.00, 'activa');
SET @IdSubasta1 = SCOPE_IDENTITY();

-- Subasta Mercedes Clase C
INSERT INTO Subasta (IdVehiculo, FechaInicio, FechaFin, PrecioInicial, IncrementoMinimo, PrecioActual, Estado)
VALUES (@IdVehiculo2, GETDATE(), DATEADD(day, 5, GETDATE()), 22000.00, 300.00, 22000.00, 'activa');
SET @IdSubasta2 = SCOPE_IDENTITY();

-- Subasta Audi A4
INSERT INTO Subasta (IdVehiculo, FechaInicio, FechaFin, PrecioInicial, IncrementoMinimo, PrecioActual, Estado)
VALUES (@IdVehiculo3, GETDATE(), DATEADD(day, 6, GETDATE()), 28000.00, 400.00, 28000.00, 'activa');
SET @IdSubasta3 = SCOPE_IDENTITY();

-- Subasta Tesla Model 3
INSERT INTO Subasta (IdVehiculo, FechaInicio, FechaFin, PrecioInicial, IncrementoMinimo, PrecioActual, Estado)
VALUES (@IdVehiculo4, GETDATE(), DATEADD(day, 10, GETDATE()), 42000.00, 600.00, 42000.00, 'activa');
SET @IdSubasta4 = SCOPE_IDENTITY();

-- Subasta VW Tiguan
INSERT INTO Subasta (IdVehiculo, FechaInicio, FechaFin, PrecioInicial, IncrementoMinimo, PrecioActual, Estado)
VALUES (@IdVehiculo5, GETDATE(), DATEADD(day, 4, GETDATE()), 24000.00, 350.00, 24000.00, 'activa');
SET @IdSubasta5 = SCOPE_IDENTITY();

-- Insertar imágenes de vehículos (URLs de ejemplo de coches reales)
-- BMW X5
INSERT INTO ImagenVehiculo (idVehiculo, ruta, nombre, activo)
VALUES 
(@IdVehiculo1, 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800', 'Vista frontal BMW X5', 1),
(@IdVehiculo1, 'https://images.unsplash.com/photo-1617531653332-bd46c24f2068?w=800', 'Interior BMW X5', 1),
(@IdVehiculo1, 'https://images.unsplash.com/photo-1617469767053-d3b523a0b982?w=800', 'Vista lateral BMW X5', 1);

-- Mercedes Clase C
INSERT INTO ImagenVehiculo (idVehiculo, ruta, nombre, activo)
VALUES 
(@IdVehiculo2, 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800', 'Vista frontal Mercedes Clase C', 1),
(@IdVehiculo2, 'https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=800', 'Interior Mercedes Clase C', 1),
(@IdVehiculo2, 'https://images.unsplash.com/photo-1609521263577-4d2d2b6e3b5e?w=800', 'Vista trasera Mercedes', 1);

-- Audi A4
INSERT INTO ImagenVehiculo (idVehiculo, ruta, nombre, activo)
VALUES 
(@IdVehiculo3, 'https://images.unsplash.com/photo-1610768764270-790fbec18178?w=800', 'Vista frontal Audi A4', 1),
(@IdVehiculo3, 'https://images.unsplash.com/photo-1614162692292-7ac56d7f3c19?w=800', 'Interior Audi A4', 1),
(@IdVehiculo3, 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800', 'Detalles Audi', 1);

-- Tesla Model 3
INSERT INTO ImagenVehiculo (idVehiculo, ruta, nombre, activo)
VALUES 
(@IdVehiculo4, 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800', 'Vista frontal Tesla Model 3', 1),
(@IdVehiculo4, 'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800', 'Interior Tesla Model 3', 1),
(@IdVehiculo4, 'https://images.unsplash.com/photo-1619682817481-e994891cd1f5?w=800', 'Panel Tesla', 1);

-- VW Tiguan
INSERT INTO ImagenVehiculo (idVehiculo, ruta, nombre, activo)
VALUES 
(@IdVehiculo5, 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800', 'Vista frontal VW Tiguan', 1),
(@IdVehiculo5, 'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800', 'Interior VW Tiguan', 1),
(@IdVehiculo5, 'https://images.unsplash.com/photo-1581540222194-0def2dda95b8?w=800', 'Vista lateral VW Tiguan', 1);

PRINT 'Insertados 5 vehículos con subastas activas e imágenes:';
PRINT '1. BMW X5 xDrive40i (7892KLM) - Subasta ID: ' + CAST(@IdSubasta1 AS VARCHAR);
PRINT '2. Mercedes-Benz Clase C 220d (5634PMN) - Subasta ID: ' + CAST(@IdSubasta2 AS VARCHAR);
PRINT '3. Audi A4 Avant 2.0 TDI (9123FGH) - Subasta ID: ' + CAST(@IdSubasta3 AS VARCHAR);
PRINT '4. Tesla Model 3 Long Range (3567TML) - Subasta ID: ' + CAST(@IdSubasta4 AS VARCHAR);
PRINT '5. Volkswagen Tiguan 2.0 TDI (8765BWC) - Subasta ID: ' + CAST(@IdSubasta5 AS VARCHAR);
PRINT '';
PRINT 'Todas las subastas están activas y finalizan entre 4 y 10 días.';

GO
