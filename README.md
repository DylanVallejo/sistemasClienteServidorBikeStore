# BikeStore - Sistema Cliente Servidor

Solucion para una tienda de bicicletas: sitio Web (ASP.NET Core MVC) que consume
una API RESTful, la cual accede a SQL Server mediante ADO.NET. Arquitectura por capas.

## Estructura del proyecto

```
trabajoGrupalFinal/
├─ scripts/                  Scripts SQL de la base de datos
│  ├─ 01_crear_bd.sql        Crea BD, tablas, PK y FK
│  └─ 02_datos_prueba.sql    Inserta datos de ejemplo
└─ src/
   ├─ BikeStore.Domain/      Entidades y DTOs (compartido por todas las capas)
   ├─ BikeStore.DataAccess/  Acceso a datos con ADO.NET (repositorios)
   ├─ BikeStore.API/         API RESTful (controladores + Swagger)
   └─ BikeStore.Web/         Sitio Web MVC que consume la API
```

## Arquitectura por capas

```
Web MVC  ->  API REST  ->  DataAccess (ADO.NET)  ->  SQL Server
(vistas)     (controllers)  (repositorios)            (BikeStoreDB)
```

- **Domain**: clases del negocio (Categoria, Bicicleta, Cliente, Venta, DetalleVenta).
- **DataAccess**: cada repositorio ejecuta consultas SQL parametrizadas con `SqlCommand`.
- **API**: expone los endpoints REST y valida los datos. Usa inyeccion de dependencias.
- **Web**: controladores MVC que llaman a la API con `HttpClient` (clase `ApiClient`).

## Requisitos

- .NET SDK 10
- SQL Server (Express) con la instancia `.\SQLEXPRESS`

## Puesta en marcha

### 1) Base de datos
Ejecutar en SQL Server, en orden:
```
scripts/01_crear_bd.sql
scripts/02_datos_prueba.sql
```
(Se pueden abrir en SSMS, o ver comandos de sqlcmd en `scripts/README.md`.)

La cadena de conexion esta en `src/BikeStore.API/appsettings.json`.
Si tu instancia es distinta a `.\SQLEXPRESS`, cambiala ahi.

> Alternativa sin SQL Server Express: se puede usar **LocalDB** (viene con
> las herramientas de .NET/Visual Studio) sin cambiar el codigo. Ver detalles
> en `AVANCE.txt`. Resumen:
> ```
> sqllocaldb start MSSQLLocalDB
> sqlcmd -S "(localdb)\MSSQLLocalDB" -i scripts/01_crear_bd.sql
> sqlcmd -S "(localdb)\MSSQLLocalDB" -i scripts/02_datos_prueba.sql
> ```
> y correr la API con la cadena de LocalDB via variable de entorno
> `ConnectionStrings__BikeStoreDB`.

### 2) Levantar la API
```
cd src/BikeStore.API
dotnet run
```
Swagger: http://localhost:5159/swagger

### 3) Levantar la Web (en otra terminal)
```
cd src/BikeStore.Web
dotnet run
```
La Web lee la URL de la API desde `appsettings.json` (`ApiBaseUrl`).

> Importante: primero la API, luego la Web. La Web NO funciona si la API esta apagada.

## Endpoints principales de la API

| Metodo | Endpoint                         | Descripcion                       |
|--------|----------------------------------|-----------------------------------|
| GET    | /api/bicicletas                  | Todas las bicicletas              |
| GET    | /api/bicicletas/{id}             | Una bicicleta                     |
| GET    | /api/bicicletas/buscar           | Buscar por nombre/categoria/marca |
| GET    | /api/bicicletas/stock-bajo       | Bicicletas con stock bajo         |
| POST   | /api/bicicletas                  | Registrar bicicleta               |
| PUT    | /api/bicicletas/{id}             | Actualizar bicicleta              |
| DELETE | /api/bicicletas/{id}             | Eliminar bicicleta                |
| CRUD   | /api/categorias                  | Administrar categorias            |
| CRUD   | /api/clientes                    | Administrar clientes              |
| GET    | /api/ventas                      | Historial de ventas               |
| GET    | /api/ventas/{id}                 | Detalle de una venta              |
| GET    | /api/ventas/cliente/{idCliente}  | Ventas de un cliente              |
| POST   | /api/ventas                      | Registrar venta (calcula IVA)     |

## Reglas de negocio

- IVA = 15% (constante en `BikeStore.Domain/ReglasNegocio.cs`).
- Al registrar una venta se descuenta el stock automaticamente (en una transaccion).
- Si no hay stock suficiente, la venta se rechaza y no se guarda nada.
