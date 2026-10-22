-- HotelDB: TipoHabitacion, Habitacion, Huesped, Reserva + datos de prueba
USE master;
GO
IF DB_ID('HotelDB') IS NOT NULL
BEGIN
    ALTER DATABASE HotelDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HotelDB;
END
GO
CREATE DATABASE HotelDB;
GO
USE HotelDB;
GO

CREATE TABLE TipoHabitacion (
    IdTipo       INT IDENTITY(1,1) PRIMARY KEY,
    Nombre       NVARCHAR(50)  NOT NULL UNIQUE,
    PrecioNoche  DECIMAL(10,2) NOT NULL CHECK (PrecioNoche > 0),
    Capacidad    INT           NOT NULL CHECK (Capacidad > 0)
);

CREATE TABLE Habitacion (
    IdHabitacion INT IDENTITY(1,1) PRIMARY KEY,
    Numero       NVARCHAR(10) NOT NULL UNIQUE,
    Piso         INT          NOT NULL,
    IdTipo       INT          NOT NULL REFERENCES TipoHabitacion(IdTipo),
    Activo       BIT          NOT NULL DEFAULT 1
);

CREATE TABLE Huesped (
    IdHuesped    INT IDENTITY(1,1) PRIMARY KEY,
    Nombres      NVARCHAR(80)  NOT NULL,
    Apellidos    NVARCHAR(80)  NOT NULL,
    Documento    NVARCHAR(15)  NOT NULL UNIQUE,
    Email        NVARCHAR(100) NULL,
    Telefono     NVARCHAR(20)  NULL,
    Activo       BIT           NOT NULL DEFAULT 1
);

CREATE TABLE Reserva (
    IdReserva     INT IDENTITY(1,1) PRIMARY KEY,
    IdHuesped     INT          NOT NULL REFERENCES Huesped(IdHuesped),
    IdHabitacion  INT          NOT NULL REFERENCES Habitacion(IdHabitacion),
    FechaIngreso  DATE         NOT NULL,
    FechaSalida   DATE         NOT NULL,
    Estado        NVARCHAR(20) NOT NULL DEFAULT 'Pendiente'
                  CHECK (Estado IN ('Pendiente','Confirmada','Cancelada','Finalizada')),
    FechaRegistro DATETIME     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_Reserva_Fechas CHECK (FechaSalida > FechaIngreso)
);
GO

INSERT INTO TipoHabitacion (Nombre, PrecioNoche, Capacidad) VALUES
('Simple', 120.00, 1),
('Doble', 180.00, 2),
('Matrimonial', 220.00, 2),
('Suite', 450.00, 4);

INSERT INTO Habitacion (Numero, Piso, IdTipo, Activo) VALUES
('101', 1, 1, 1),
('102', 1, 1, 1),
('103', 1, 2, 1),
('201', 2, 2, 1),
('202', 2, 3, 1),
('203', 2, 3, 1),
('301', 3, 4, 1),
('302', 3, 4, 0);   -- inactiva (eliminación lógica)

INSERT INTO Huesped (Nombres, Apellidos, Documento, Email, Telefono, Activo) VALUES
('Ana',    'Torres Ruiz',     '45871236', 'ana.torres@mail.com',   '987654321', 1),
('Luis',   'Quispe Mamani',   '41236587', 'luis.quispe@mail.com',  '912345678', 1),
('María',  'Fernández Díaz',  '47856321', 'maria.fd@mail.com',     '956321478', 1),
('Jorge',  'Ramírez Soto',    '40125896', NULL,                    '934567812', 1),
('Carmen', 'Vargas León',     '43698521', 'carmen.vl@mail.com',    NULL,        0); -- inactivo

-- Fechas relativas a hoy para que siempre haya cruces que probar
DECLARE @hoy DATE = CAST(GETDATE() AS DATE);
INSERT INTO Reserva (IdHuesped, IdHabitacion, FechaIngreso, FechaSalida, Estado) VALUES
(1, 1, DATEADD(DAY, -10, @hoy), DATEADD(DAY, -7, @hoy), 'Finalizada'),
(2, 3, DATEADD(DAY,   1, @hoy), DATEADD(DAY,  4, @hoy), 'Confirmada'),
(3, 5, DATEADD(DAY,   2, @hoy), DATEADD(DAY,  5, @hoy), 'Pendiente'),
(4, 7, DATEADD(DAY,   3, @hoy), DATEADD(DAY,  6, @hoy), 'Cancelada'),
(1, 2, DATEADD(DAY,   5, @hoy), DATEADD(DAY,  8, @hoy), 'Pendiente');
GO
