# RT221663_desafio03 — Sistema de Pedidos

**DES104 – Desarrollo de Software Empresarial · Desafío práctico 3**
Universidad Don Bosco · Grupo 02L · Ciclo 02-2026
Autor: **Nelson Steven Rivas Tobar (RT221663)**

API REST en **ASP.NET Core 8** con **arquitectura de N capas** para gestionar Clientes, Productos y Pedidos.
Usa **Entity Framework Core** (SQL Server, migraciones), **Data Annotations**, **Docker** y pruebas unitarias con **xUnit**.

---

## 1. Arquitectura

```
RT221663_desafio03
├── src
│   ├── Desafio03.Entidades     → Modelos (Cliente, Producto, Pedido) con Data Annotations
│   │                             e interfaces de repositorios / unidad de trabajo
│   ├── Desafio03.AccesoDatos   → DbContext de EF Core, relaciones, migraciones, repositorios
│   ├── Desafio03.Negocio       → Servicios, reglas de negocio, DTOs, validaciones
│   └── Desafio03.API           → Controladores REST, middleware de errores, Swagger
├── tests
│   └── Desafio03.Pruebas       → Pruebas unitarias xUnit (34 pruebas)
├── scripts
│   ├── PedidosDB.sql           → Script de la base de datos (opcional)
│   └── pruebas-api.http        → Peticiones de ejemplo para probar la API
├── Dockerfile
└── docker-compose.yml
```

Dependencias entre capas:

```
API ──► Negocio ──► Entidades ◄── AccesoDatos
 └──────────────────────────────────┘ (solo para registrar el DbContext)
```

La capa de Negocio trabaja contra **interfaces** (`IUnidadDeTrabajo`, `IRepositorioClientes`, …),
por eso las pruebas unitarias no necesitan base de datos.

### Modelos y relaciones

| Modelo   | Campos                                                                 |
|----------|------------------------------------------------------------------------|
| Cliente  | Id, Nombre, Apellido, Email                                            |
| Producto | Id, Nombre, Descripcion, Precio, Stock                                 |
| Pedido   | Id, FechaPedido, Cantidad, ClienteId, ProductoId, PrecioUnitario, Total |

- Cliente **1 — N** Pedidos
- Producto **1 — N** Pedidos

### Validaciones (Data Annotations)

| Campo             | Regla                                                       |
|-------------------|-------------------------------------------------------------|
| Nombre / Apellido | Obligatorio, 2–50 caracteres, solo letras y espacios        |
| Email             | Obligatorio, formato válido, máx. 100 caracteres            |
| Nombre producto   | Obligatorio, 2–100 caracteres                               |
| Descripción       | Obligatoria, 5–500 caracteres                               |
| Precio            | Entre 0.01 y 999,999.99                                     |
| Stock             | Entre 0 y 100,000                                           |
| Cantidad          | Entre 1 y 1,000                                             |
| ClienteId / ProductoId | Obligatorios (mayores que 0)                           |

### Reglas de negocio

1. El **email** del cliente es único (no distingue mayúsculas).
2. Un pedido solo se crea si el **cliente y el producto existen**.
3. La **cantidad no puede superar el stock** disponible.
4. Al crear un pedido se **descuenta el stock**; al modificarlo se ajusta; al eliminarlo se **devuelve**.
5. La **fecha del pedido no puede ser futura** (si no se envía, se usa la fecha actual).
6. El **total** se calcula como precio unitario × cantidad.
7. No se puede **eliminar** un cliente o producto que tenga pedidos.

### Códigos de respuesta

| Situación                    | Código |
|------------------------------|--------|
| Consulta / actualización OK  | 200    |
| Creación                     | 201    |
| Eliminación                  | 204    |
| Datos no válidos             | 400    |
| Registro inexistente         | 404    |
| Regla de negocio incumplida  | 409    |

---

## 2. Endpoints

| Método | Ruta                          | Descripción                         |
|--------|-------------------------------|-------------------------------------|
| GET    | `/api/clientes`               | Lista clientes                      |
| GET    | `/api/clientes/{id}`          | Obtiene un cliente                  |
| GET    | `/api/clientes/{id}/pedidos`  | Pedidos de un cliente               |
| POST   | `/api/clientes`               | Crea un cliente                     |
| PUT    | `/api/clientes/{id}`          | Actualiza un cliente                |
| DELETE | `/api/clientes/{id}`          | Elimina un cliente                  |
| GET    | `/api/productos`              | Lista productos                     |
| GET    | `/api/productos/{id}`         | Obtiene un producto                 |
| POST   | `/api/productos`              | Crea un producto                    |
| PUT    | `/api/productos/{id}`         | Actualiza un producto               |
| DELETE | `/api/productos/{id}`         | Elimina un producto                 |
| GET    | `/api/pedidos`                | Lista pedidos                       |
| GET    | `/api/pedidos/{id}`           | Obtiene un pedido                   |
| POST   | `/api/pedidos`                | Crea un pedido (descuenta stock)    |
| PUT    | `/api/pedidos/{id}`           | Modifica un pedido (ajusta stock)   |
| DELETE | `/api/pedidos/{id}`           | Cancela un pedido (devuelve stock)  |
| GET    | `/health`                     | Estado de la API                    |

---

## 3. Ejecutar con Docker (recomendado)

Requisito: **Docker Desktop** abierto.

```bash
docker compose up --build
```

Esto levanta **SQL Server 2022** y la **API**. Al arrancar, la API aplica la migración
`CreacionInicial` y carga datos de ejemplo (3 clientes, 4 productos, 2 pedidos).

- Swagger: **http://localhost:8080/swagger**
- Detener: `Ctrl + C` y luego `docker compose down`
- Borrar también la base de datos: `docker compose down -v`

> La primera vez tarda unos minutos porque descarga las imágenes de .NET y SQL Server.

---

## 4. Ejecutar sin Docker (Visual Studio / `dotnet run`)

1. Levanta solo la base de datos con Docker:
   ```bash
   docker compose up -d sqlserver
   ```
   (o usa tu propio SQL Server y cambia `ConexionSql` en `src/Desafio03.API/appsettings.json`;
   con LocalDB sería `Server=(localdb)\\mssqllocaldb;Database=PedidosDB;Trusted_Connection=True;TrustServerCertificate=True`).
2. Ejecuta la API:
   ```bash
   dotnet run --project src/Desafio03.API
   ```
3. Abre **http://localhost:5080/swagger**

---

## 5. Pruebas unitarias (xUnit)

```bash
dotnet test
```

O dentro de Docker:

```bash
docker compose --profile pruebas run --rm pruebas
```

| Caso del enunciado                          | Prueba(s)                                                        |
|---------------------------------------------|------------------------------------------------------------------|
| Creación de un cliente                      | `CrearCliente_ConDatosValidos_GuardaElCliente`                   |
| Validación de datos incorrectos de un cliente | `CrearCliente_ConDatosIncorrectos_LanzaValidacionConErroresPorCampo`, `ValidarCliente_EmailConFormatoIncorrecto_…`, `ValidarCliente_NombreDemasiadoLargo_…` |
| Creación de un producto                     | `CrearProducto_ConDatosValidos_GuardaElProducto`                 |
| Validación del precio y stock               | `CrearProducto_ConPrecioFueraDeRango_…`, `CrearProducto_ConStockFueraDeRango_…`, `ValidarProducto_PrecioYStockEnElLimite_…` |
| Creación de un pedido                       | `CrearPedido_ConDatosValidos_GuardaPedidoYCalculaTotal`          |
| Consulta de un pedido existente             | `ObtenerPedido_Existente_DevuelveSusDatos`                       |
| Consulta de un registro inexistente         | `ObtenerPedido_Inexistente_…`, `ObtenerCliente_Inexistente_…`, `ObtenerProducto_Inexistente_…` |
| Validación de reglas de negocio             | `CrearPedido_CantidadMayorAlStock_…`, `CrearCliente_ConEmailRepetido_…`, `CrearPedido_ConFechaFutura_…`, `EliminarPedido_DevuelveLasUnidadesAlStock`, `EliminarCliente_ConPedidos_…`, entre otras |

---

## 6. Migraciones de EF Core

La migración inicial ya está incluida en `src/Desafio03.AccesoDatos/Migraciones`.
Para crear nuevas migraciones (con SQL Server corriendo):

```bash
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef migrations add NombreDeLaMigracion --project src/Desafio03.AccesoDatos --startup-project src/Desafio03.API --output-dir Migraciones
dotnet ef database update --project src/Desafio03.AccesoDatos --startup-project src/Desafio03.API
```

---

## 7. Si solo tienes .NET 10

La solución usa .NET 8. Si tu equipo solo tiene el SDK de .NET 10:

1. En `Directory.Build.props` cambia `net8.0` por `net10.0`.
2. En `Directory.Packages.props` cambia las versiones `8.0.10` de EF Core por `10.0.*`.
3. En el `Dockerfile` cambia `sdk:8.0` y `aspnet:8.0` por `sdk:10.0` y `aspnet:10.0`.

Con Docker no hace falta tener .NET instalado: la imagen ya trae el SDK.
