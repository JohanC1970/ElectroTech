using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace ElectroTech.Helpers
{
    /// <summary>
    /// Clase para manejar la configuración de la conexión a la base de datos.
    /// Permite cambiar la configuración de conexión en tiempo de ejecución.
    /// </summary>
    public static class DbSettings
    {
        /// <summary>
        /// Obtiene la cadena de conexión actual a la base de datos Oracle.
        /// </summary>
        /// <returns>La cadena de conexión configurada.</returns>
        public static string GetConnectionString()
        {
            try
            {
                return ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener la cadena de conexión");
                throw new Exception("No se pudo obtener la configuración de conexión a la base de datos.", ex);
            }
        }

        /// <summary>
        /// Actualiza la cadena de conexión a la base de datos Oracle.
        /// </summary>
        /// <param name="host">Servidor de la base de datos.</param>
        /// <param name="port">Puerto de conexión.</param>
        /// <param name="serviceName">Nombre del servicio Oracle.</param>
        /// <param name="userId">Usuario de la base de datos.</param>
        /// <param name="password">Contraseña del usuario.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public static bool UpdateConnectionString(string host, string port, string serviceName, string userId, string password)
        {
            try
            {
                // Construir la nueva cadena de conexión
                string newConnectionString =
                    $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))" +
                    $"(CONNECT_DATA=(SERVICE_NAME={serviceName})));User Id={userId};Password={password};";

                // Actualizar el archivo de configuración
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConnectionStringsSection connectionStringsSection = config.ConnectionStrings;

                connectionStringsSection.ConnectionStrings["OracleConnection"].ConnectionString = newConnectionString;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("connectionStrings");

                // Actualizar también la sección oracle.manageddataaccess.client
                UpdateOracleDataSource(host, port, serviceName);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al actualizar la cadena de conexión");
                return false;
            }
        }

        /// <summary>
        /// Actualiza la configuración del datasource de Oracle en el archivo de configuración.
        /// </summary>
        /// <param name="host">Servidor de la base de datos.</param>
        /// <param name="port">Puerto de conexión.</param>
        /// <param name="serviceName">Nombre del servicio Oracle.</param>
        private static void UpdateOracleDataSource(string host, string port, string serviceName)
        {
            try
            {
                string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                XmlDocument doc = new XmlDocument();
                doc.Load(configFile);

                // Actualizar el descriptor del dataSource
                string descriptor =
                    $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))" +
                    $"(CONNECT_DATA=(SERVICE_NAME={serviceName})))";

                XmlNode node = doc.SelectSingleNode("//dataSource[@alias='ElectroTechDB']");
                if (node != null)
                {
                    node.Attributes["descriptor"].Value = descriptor;
                    doc.Save(configFile);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al actualizar la configuración de Oracle");
                throw;
            }
        }

        /// <summary>
        /// Verifica la conexión a la base de datos con las credenciales proporcionadas.
        /// </summary>
        /// <param name="host">Servidor de la base de datos.</param>
        /// <param name="port">Puerto de conexión.</param>
        /// <param name="serviceName">Nombre del servicio Oracle.</param>
        /// <param name="userId">Usuario de la base de datos.</param>
        /// <param name="password">Contraseña del usuario.</param>
        /// <returns>True si la conexión es exitosa, False en caso contrario.</returns>
        public static bool TestConnection(string host, string port, string serviceName, string userId, string password)
        {
            try
            {
                string connectionString =
                    $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))" +
                    $"(CONNECT_DATA=(SERVICE_NAME={serviceName})));User Id={userId};Password={password};";

                using (Oracle.ManagedDataAccess.Client.OracleConnection connection =
                    new Oracle.ManagedDataAccess.Client.OracleConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al probar la conexión a la base de datos");
                return false;
            }
        }

        /// <summary>
        /// Crea un script SQL para la inicialización de la base de datos.
        /// </summary>
        /// <returns>True si la creación es exitosa, False en caso contrario.</returns>
        public static bool CreateDatabaseScript()
        {
            try
            {
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
                string scriptFile = Path.Combine(scriptPath, "CreacionBaseDatos.sql");

                // Verificar si el directorio existe, si no, crearlo
                if (!Directory.Exists(scriptPath))
                {
                    Directory.CreateDirectory(scriptPath);
                }

                // Crear el archivo SQL con el script de creación
                using (StreamWriter writer = new StreamWriter(scriptFile, false))
                {
                    writer.Write(GetDatabaseCreationScript());
                }

                return File.Exists(scriptFile);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al crear el script de base de datos");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el script de creación de la base de datos.
        /// </summary>
        /// <returns>El script SQL como string.</returns>
        private static string GetDatabaseCreationScript()
        {
            // Este método retorna el script que ya se generó en el archivo CreacionBaseDatos.sql
            // Aquí se coloca el contenido del script como string para poder generarlo desde código
            return @"-- Script para la creación de las tablas y secuencias de la base de datos ElectroTech
-- Autor: ElectroTech
-- Fecha: 2025-04-27

-- Eliminar tablas si existen para evitar conflictos
BEGIN
    -- Eliminar relaciones primero
    EXECUTE IMMEDIATE 'DROP TABLE BitacoraUsuario CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE DetalleVenta CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE DetalleCompra CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE Devolucion CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE Inventario CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    -- Eliminar tablas principales
    EXECUTE IMMEDIATE 'DROP TABLE Usuario CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE Producto CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE Categoria CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE Proveedor CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE Cliente CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE Empleado CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE Venta CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE Compra CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP TABLE MetodoPago CASCADE CONSTRAINTS';
    EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- Eliminar secuencias si existen
BEGIN
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_USUARIO';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_BITACORA';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_PRODUCTO';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_CATEGORIA';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_PROVEEDOR';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_CLIENTE';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_EMPLEADO';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_VENTA';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_COMPRA';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_DETALLE_VENTA';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_DETALLE_COMPRA';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_DEVOLUCION';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_INVENTARIO';
    EXCEPTION WHEN OTHERS THEN NULL;
    
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_METODO_PAGO';
    EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- Crear secuencias para generar IDs únicos
CREATE SEQUENCE SEQ_USUARIO START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_BITACORA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_PRODUCTO START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_CATEGORIA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_PROVEEDOR START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_CLIENTE START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_EMPLEADO START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_VENTA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_COMPRA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_DETALLE_VENTA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_DETALLE_COMPRA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_DEVOLUCION START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_INVENTARIO START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_METODO_PAGO START WITH 1 INCREMENT BY 1;

-- Crear tablas
-- Tabla Usuario
CREATE TABLE Usuario (
    idUsuario NUMBER PRIMARY KEY,
    nombreUsuario VARCHAR2(50) NOT NULL UNIQUE,
    clave VARCHAR2(100) NOT NULL,
    nivel NUMBER(1) NOT NULL,
    nombreCompleto VARCHAR2(100) NOT NULL,
    correo VARCHAR2(100) NOT NULL,
    estado CHAR(1) DEFAULT 'A' NOT NULL,
    fechaCreacion DATE DEFAULT SYSDATE NOT NULL,
    ultimaConexion DATE,
    CONSTRAINT chk_usuario_nivel CHECK (nivel IN (1, 2, 3)),
    CONSTRAINT chk_usuario_estado CHECK (estado IN ('A', 'I'))
);

-- Tabla BitacoraUsuario
CREATE TABLE BitacoraUsuario (
    idBitacora NUMBER PRIMARY KEY,
    idUsuario NUMBER NOT NULL,
    tipoAccion CHAR(1) NOT NULL,
    fechaHora TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    ipAcceso VARCHAR2(45), -- Soporte para IPv6
    CONSTRAINT fk_bitacora_usuario FOREIGN KEY (idUsuario) REFERENCES Usuario(idUsuario),
    CONSTRAINT chk_bitacora_tipo CHECK (tipoAccion IN ('E', 'S')) -- E: Entrada, S: Salida
);

-- Tabla Categoria
CREATE TABLE Categoria (
    idCategoria NUMBER PRIMARY KEY,
    nombre VARCHAR2(50) NOT NULL UNIQUE,
    descripcion VARCHAR2(200),
    activa CHAR(1) DEFAULT 'S' NOT NULL,
    CONSTRAINT chk_categoria_activa CHECK (activa IN ('S', 'N')) -- S: Sí, N: No
);

-- Tabla Producto
CREATE TABLE Producto (
    idProducto NUMBER PRIMARY KEY,
    codigo VARCHAR2(20) NOT NULL UNIQUE,
    nombre VARCHAR2(100) NOT NULL,
    descripcion VARCHAR2(500),
    idCategoria NUMBER NOT NULL,
    idMarca NUMBER,
    modelo VARCHAR2(50),
    precioCompra NUMBER(10,2) NOT NULL,
    precioVenta NUMBER(10,2) NOT NULL,
    stockMinimo NUMBER(5) DEFAULT 5 NOT NULL,
    ubicacionAlmacen VARCHAR2(50),
    imagen BLOB,
    activo CHAR(1) DEFAULT 'S' NOT NULL,
    CONSTRAINT fk_producto_categoria FOREIGN KEY (idCategoria) REFERENCES Categoria(idCategoria),
    CONSTRAINT chk_producto_activo CHECK (activo IN ('S', 'N')), -- S: Sí, N: No
    CONSTRAINT chk_producto_precio CHECK (precioVenta > precioCompra) -- Regla de negocio
);

-- Tabla Proveedor
CREATE TABLE Proveedor (
    idProveedor NUMBER PRIMARY KEY,
    nombre VARCHAR2(100) NOT NULL,
    direccion VARCHAR2(200),
    telefono VARCHAR2(20),
    correo VARCHAR2(100),
    contacto VARCHAR2(100),
    condicionesPago VARCHAR2(200),
    activo CHAR(1) DEFAULT 'S' NOT NULL,
    CONSTRAINT chk_proveedor_activo CHECK (activo IN ('S', 'N')) -- S: Sí, N: No
);

-- Tabla Cliente
CREATE TABLE Cliente (
    idCliente NUMBER PRIMARY KEY,
    tipoDocumento VARCHAR2(3) NOT NULL, -- DNI, RUC, CE, etc.
    numeroDocumento VARCHAR2(20) NOT NULL,
    nombre VARCHAR2(100) NOT NULL,
    apellido VARCHAR2(100) NOT NULL,
    direccion VARCHAR2(200),
    telefono VARCHAR2(20),
    correo VARCHAR2(100),
    fechaRegistro DATE DEFAULT SYSDATE NOT NULL,
    activo CHAR(1) DEFAULT 'S' NOT NULL,
    CONSTRAINT chk_cliente_activo CHECK (activo IN ('S', 'N')), -- S: Sí, N: No
    CONSTRAINT uk_cliente_documento UNIQUE (tipoDocumento, numeroDocumento)
);

-- Tabla Empleado
CREATE TABLE Empleado (
    idEmpleado NUMBER PRIMARY KEY,
    tipoDocumento VARCHAR2(3) NOT NULL, -- DNI, CE, etc.
    numeroDocumento VARCHAR2(20) NOT NULL,
    nombre VARCHAR2(100) NOT NULL,
    apellido VARCHAR2(100) NOT NULL,
    direccion VARCHAR2(200),
    telefono VARCHAR2(20),
    fechaContratacion DATE DEFAULT SYSDATE NOT NULL,
    salarioBase NUMBER(10,2) NOT NULL,
    idUsuario NUMBER,
    activo CHAR(1) DEFAULT 'S' NOT NULL,
    CONSTRAINT fk_empleado_usuario FOREIGN KEY (idUsuario) REFERENCES Usuario(idUsuario),
    CONSTRAINT chk_empleado_activo CHECK (activo IN ('S', 'N')), -- S: Sí, N: No
    CONSTRAINT uk_empleado_documento UNIQUE (tipoDocumento, numeroDocumento),
    CONSTRAINT uk_empleado_usuario UNIQUE (idUsuario)
);

-- Tabla MetodoPago
CREATE TABLE MetodoPago (
    idMetodoPago NUMBER PRIMARY KEY,
    nombre VARCHAR2(50) NOT NULL UNIQUE,
    descripcion VARCHAR2(200),
    activo CHAR(1) DEFAULT 'S' NOT NULL,
    CONSTRAINT chk_metodo_pago_activo CHECK (activo IN ('S', 'N')) -- S: Sí, N: No
);

-- Tabla Venta
CREATE TABLE Venta (
    idVenta NUMBER PRIMARY KEY,
    numeroFactura VARCHAR2(20) NOT NULL UNIQUE,
    fecha DATE DEFAULT SYSDATE NOT NULL,
    idCliente NUMBER NOT NULL,
    idEmpleado NUMBER NOT NULL,
    idMetodoPago NUMBER NOT NULL,
    subtotal NUMBER(10,2) NOT NULL,
    descuento NUMBER(10,2) DEFAULT 0 NOT NULL,
    impuestos NUMBER(10,2) DEFAULT 0 NOT NULL,
    total NUMBER(10,2) NOT NULL,
    observaciones VARCHAR2(500),
    estado CHAR(1) DEFAULT 'C' NOT NULL, -- C: Completada, A: Anulada, P: Pendiente
    CONSTRAINT fk_venta_cliente FOREIGN KEY (idCliente) REFERENCES Cliente(idCliente),
    CONSTRAINT fk_venta_empleado FOREIGN KEY (idEmpleado) REFERENCES Empleado(idEmpleado),
    CONSTRAINT fk_venta_metodo_pago FOREIGN KEY (idMetodoPago) REFERENCES MetodoPago(idMetodoPago),
    CONSTRAINT chk_venta_estado CHECK (estado IN ('C', 'A', 'P')),
    CONSTRAINT chk_venta_descuento CHECK (descuento <= subtotal * 0.3) -- Máximo 30% descuento
);

-- Tabla DetalleVenta
CREATE TABLE DetalleVenta (
    idDetalleVenta NUMBER PRIMARY KEY,
    idVenta NUMBER NOT NULL,
    idProducto NUMBER NOT NULL,
    cantidad NUMBER(5) NOT NULL,
    precioUnitario NUMBER(10,2) NOT NULL,
    descuento NUMBER(10,2) DEFAULT 0 NOT NULL,
    subtotal NUMBER(10,2) NOT NULL,
    CONSTRAINT fk_detalle_venta_venta FOREIGN KEY (idVenta) REFERENCES Venta(idVenta),
    CONSTRAINT fk_detalle_venta_producto FOREIGN KEY (idProducto) REFERENCES Producto(idProducto),
    CONSTRAINT chk_detalle_venta_cantidad CHECK (cantidad > 0),
    CONSTRAINT chk_detalle_venta_precio CHECK (precioUnitario > 0)
);

-- Tabla Compra
CREATE TABLE Compra (
    idCompra NUMBER PRIMARY KEY,
    numeroOrden VARCHAR2(20) NOT NULL UNIQUE,
    fecha DATE DEFAULT SYSDATE NOT NULL,
    idProveedor NUMBER NOT NULL,
    subtotal NUMBER(10,2) NOT NULL,
    impuestos NUMBER(10,2) DEFAULT 0 NOT NULL,
    total NUMBER(10,2) NOT NULL,
    estado CHAR(1) DEFAULT 'P' NOT NULL, -- P: Pendiente, R: Recibida, C: Cancelada
    observaciones VARCHAR2(500),
    CONSTRAINT fk_compra_proveedor FOREIGN KEY (idProveedor) REFERENCES Proveedor(idProveedor),
    CONSTRAINT chk_compra_estado CHECK (estado IN ('P', 'R', 'C'))
);

-- Tabla DetalleCompra
CREATE TABLE DetalleCompra (
    idDetalleCompra NUMBER PRIMARY KEY,
    idCompra NUMBER NOT NULL,
    idProducto NUMBER NOT NULL,
    cantidad NUMBER(5) NOT NULL,
    precioUnitario NUMBER(10,2) NOT NULL,
    subtotal NUMBER(10,2) NOT NULL,
    CONSTRAINT fk_detalle_compra_compra FOREIGN KEY (idCompra) REFERENCES Compra(idCompra),
    CONSTRAINT fk_detalle_compra_producto FOREIGN KEY (idProducto) REFERENCES Producto(idProducto),
    CONSTRAINT chk_detalle_compra_cantidad CHECK (cantidad > 0),
    CONSTRAINT chk_detalle_compra_precio CHECK (precioUnitario > 0)
);

-- Tabla Devolucion
CREATE TABLE Devolucion (
    idDevolucion NUMBER PRIMARY KEY,
    idVenta NUMBER NOT NULL,
    fecha DATE DEFAULT SYSDATE NOT NULL,
    motivo VARCHAR2(500) NOT NULL,
    montoDevuelto NUMBER(10,2) NOT NULL,
    estado CHAR(1) DEFAULT 'P' NOT NULL, -- P: Procesada, R: Rechazada
    CONSTRAINT fk_devolucion_venta FOREIGN KEY (idVenta) REFERENCES Venta(idVenta),
    CONSTRAINT chk_devolucion_estado CHECK (estado IN ('P', 'R')),
    CONSTRAINT chk_devolucion_fecha CHECK (fecha - (SELECT fecha FROM Venta WHERE idVenta = Devolucion.idVenta) <= 30) -- Máximo 30 días
);

-- Tabla Inventario
CREATE TABLE Inventario (
    idInventario NUMBER PRIMARY KEY,
    idProducto NUMBER NOT NULL,
    cantidadDisponible NUMBER(5) DEFAULT 0 NOT NULL,
    ultimaActualizacion DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT fk_inventario_producto FOREIGN KEY (idProducto) REFERENCES Producto(idProducto),
    CONSTRAINT uk_inventario_producto UNIQUE (idProducto)
);

-- Inserciones iniciales para datos de configuración
-- Insertar métodos de pago iniciales
INSERT INTO MetodoPago (idMetodoPago, nombre, descripcion, activo) 
VALUES (SEQ_METODO_PAGO.NEXTVAL, 'Efectivo', 'Pago en efectivo', 'S');

INSERT INTO MetodoPago (idMetodoPago, nombre, descripcion, activo) 
VALUES (SEQ_METODO_PAGO.NEXTVAL, 'Tarjeta de Crédito', 'Pago con tarjeta de crédito', 'S');

INSERT INTO MetodoPago (idMetodoPago, nombre, descripcion, activo) 
VALUES (SEQ_METODO_PAGO.NEXTVAL, 'Tarjeta de Débito', 'Pago con tarjeta de débito', 'S');

INSERT INTO MetodoPago (idMetodoPago, nombre, descripcion, activo) 
VALUES (SEQ_METODO_PAGO.NEXTVAL, 'Transferencia Bancaria', 'Pago por transferencia bancaria', 'S');

-- Insertar categorías iniciales
INSERT INTO Categoria (idCategoria, nombre, descripcion, activa) 
VALUES (SEQ_CATEGORIA.NEXTVAL, 'Computadoras', 'Computadoras de escritorio y portátiles', 'S');

INSERT INTO Categoria (idCategoria, nombre, descripcion, activa) 
VALUES (SEQ_CATEGORIA.NEXTVAL, 'Smartphones', 'Teléfonos inteligentes', 'S');

INSERT INTO Categoria (idCategoria, nombre, descripcion, activa) 
VALUES (SEQ_CATEGORIA.NEXTVAL, 'Tablets', 'Tabletas electrónicas', 'S');

INSERT INTO Categoria (idCategoria, nombre, descripcion, activa) 
VALUES (SEQ_CATEGORIA.NEXTVAL, 'Accesorios', 'Accesorios para dispositivos electrónicos', 'S');

INSERT INTO Categoria (idCategoria, nombre, descripcion, activa) 
VALUES (SEQ_CATEGORIA.NEXTVAL, 'Componentes', 'Componentes para computadoras', 'S');

-- Insertar usuario administrador inicial (contraseña: Admin123)
INSERT INTO Usuario (idUsuario, nombreUsuario, clave, nivel, nombreCompleto, correo, estado, fechaCreacion) 
VALUES (SEQ_USUARIO.NEXTVAL, 'admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 1, 'Administrador del Sistema', 'admin@electrotech.com', 'A', SYSDATE);

-- Insertar empleado para el administrador
INSERT INTO Empleado (idEmpleado, tipoDocumento, numeroDocumento, nombre, apellido, direccion, telefono, fechaContratacion, salarioBase, idUsuario, activo) 
VALUES (SEQ_EMPLEADO.NEXTVAL, 'DNI', '12345678', 'Administrador', 'Principal', 'Av. Principal 123', '999888777', SYSDATE, 5000, 1, 'S');

-- Crear triggers
-- Trigger para validar stock al vender
CREATE OR REPLACE TRIGGER trg_verificar_stock
BEFORE INSERT ON DetalleVenta
FOR EACH ROW
DECLARE
    v_stock NUMBER;
BEGIN
    -- Obtener el stock actual del producto
    SELECT cantidadDisponible INTO v_stock
    FROM Inventario
    WHERE idProducto = :NEW.idProducto;
    
    -- Verificar si hay suficiente stock
    IF v_stock < :NEW.cantidad THEN
        RAISE_APPLICATION_ERROR(-20001, 'No hay suficiente stock disponible para el producto');
    END IF;
END;
/

-- Trigger para actualizar stock al vender
CREATE OR REPLACE TRIGGER trg_actualizar_stock_venta
AFTER INSERT ON DetalleVenta
FOR EACH ROW
BEGIN
    -- Restar del inventario
    UPDATE Inventario
    SET cantidadDisponible = cantidadDisponible - :NEW.cantidad,
        ultimaActualizacion = SYSDATE
    WHERE idProducto = :NEW.idProducto;
END;
/

-- Trigger para actualizar stock al comprar
CREATE OR REPLACE TRIGGER trg_actualizar_stock_compra
AFTER INSERT ON DetalleCompra
FOR EACH ROW
DECLARE
    v_count NUMBER;
BEGIN
    -- Verificar si la compra está en estado 'Recibida'
    SELECT COUNT(*) INTO v_count
    FROM Compra
    WHERE idCompra = :NEW.idCompra AND estado = 'R';
    
    -- Solo actualizar stock si la compra está recibida
    IF v_count > 0 THEN
        -- Verificar si el producto ya existe en el inventario
        SELECT COUNT(*) INTO v_count
        FROM Inventario
        WHERE idProducto = :NEW.idProducto;
        
        IF v_count > 0 THEN
            -- Actualizar inventario existente
            UPDATE Inventario
            SET cantidadDisponible = cantidadDisponible + :NEW.cantidad,
                ultimaActualizacion = SYSDATE
            WHERE idProducto = :NEW.idProducto;
        ELSE
            -- Crear nuevo registro en inventario
            INSERT INTO Inventario (idInventario, idProducto, cantidadDisponible, ultimaActualizacion)
            VALUES (SEQ_INVENTARIO.NEXTVAL, :NEW.idProducto, :NEW.cantidad, SYSDATE);
        END IF;
    END IF;
END;
/

-- Trigger para calcular comisión al empleado por venta
CREATE OR REPLACE TRIGGER trg_calcular_comision
AFTER INSERT ON Venta
FOR EACH ROW
DECLARE
    v_comision NUMBER(10,2);
BEGIN
    -- Calcular comisión del 2% sobre el total de la venta
    v_comision := :NEW.total * 0.02;
    
    -- Aquí podría registrarse en una tabla de comisiones si se desea
    -- Por ahora solo se muestra como ejemplo
    DBMS_OUTPUT.PUT_LINE('Comisión calculada para el empleado ' || :NEW.idEmpleado || ': ' || v_comision);
END;
/

-- Trigger para validar devoluciones (máximo 30 días)
CREATE OR REPLACE TRIGGER trg_validar_devolucion
BEFORE INSERT ON Devolucion
FOR EACH ROW
DECLARE
    v_fecha_venta DATE;
    v_dias NUMBER;
BEGIN
    -- Obtener fecha de la venta
    SELECT fecha INTO v_fecha_venta
    FROM Venta
    WHERE idVenta = :NEW.idVenta;
    
    -- Calcular días transcurridos
    v_dias := :NEW.fecha - v_fecha_venta;
    
    -- Validar que no hayan pasado más de 30 días
    IF v_dias > 30 THEN
        RAISE_APPLICATION_ERROR(-20002, 'No se pueden realizar devoluciones después de 30 días de la compra');
    END IF;
END;
/

-- Trigger para bloquear usuarios inactivos por más de 90 días
CREATE OR REPLACE TRIGGER trg_bloquear_usuarios_inactivos
BEFORE UPDATE OF ultimaConexion ON Usuario
FOR EACH ROW
DECLARE
    v_dias NUMBER;
BEGIN
    -- Calcular días desde la última conexión anterior
    IF :OLD.ultimaConexion IS NOT NULL THEN
        v_dias := SYSDATE - :OLD.ultimaConexion;
        
        -- Bloquear usuario si han pasado más de 90 días
        IF v_dias > 90 AND :NEW.estado = 'A' THEN
            :NEW.estado := 'I';
        END IF;
    END IF;
END;
/

COMMIT;
";
        }

        /// <summary>
        /// Ejecuta un script SQL en la base de datos.
        /// </summary>
        /// <param name="scriptPath">Ruta del archivo script a ejecutar.</param>
        /// <returns>True si la ejecución es exitosa, False en caso contrario.</returns>
        public static bool ExecuteScript(string scriptPath)
        {
            try
            {
                if (!File.Exists(scriptPath))
                {
                    Logger.LogError($"El archivo de script no existe: {scriptPath}");
                    return false;
                }

                string script = File.ReadAllText(scriptPath);
                string connectionString = GetConnectionString();

                using (Oracle.ManagedDataAccess.Client.OracleConnection connection =
                    new Oracle.ManagedDataAccess.Client.OracleConnection(connectionString))
                {
                    connection.Open();

                    // Dividir el script por el delimitador '/'
                    string[] commands = script.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                    using (Oracle.ManagedDataAccess.Client.OracleCommand command = connection.CreateCommand())
                    {
                        foreach (string commandText in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(commandText))
                            {
                                command.CommandText = commandText;
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al ejecutar el script: {scriptPath}");
                return false;
            }
        }
    }
}