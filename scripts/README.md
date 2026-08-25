# Scripts SQL - BikeStore

Ejecutar en este orden:

1. `01_crear_bd.sql` - crea la base de datos `BikeStoreDB`, las tablas, las llaves
   primarias y foraneas, y restricciones.
2. `02_datos_prueba.sql` - inserta categorias, bicicletas y clientes de ejemplo.

## Opcion A: SQL Server Management Studio (SSMS)
Abrir cada archivo y ejecutar (F5).

## Opcion B: linea de comandos (sqlcmd)
Con autenticacion de Windows en la instancia SQLEXPRESS:

```
sqlcmd -S .\SQLEXPRESS -E -i 01_crear_bd.sql
sqlcmd -S .\SQLEXPRESS -E -i 02_datos_prueba.sql
```

> Nota: el servicio de SQL Server debe estar iniciado.
> Para iniciarlo (como administrador): `net start MSSQL$SQLEXPRESS`
