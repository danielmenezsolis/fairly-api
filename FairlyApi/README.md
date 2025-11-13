# Fairly API - Expense Sharing App

API REST desarrollada en **C# (.NET 8)** para una aplicación de gastos compartidos.

## Características

- ✅ Gestión de usuarios
- ✅ Creación de grupos
- ✅ División equitativa de gastos
- ✅ División personalizada de gastos
- ✅ Cálculo automático de balances
- ✅ Algoritmo de simplificación de deudas

## Tecnologías

- .NET 8
- Entity Framework Core
- PostgreSQL (Supabase)
- Swagger/OpenAPI

## Configuración

1. Clonar el repositorio:
```bash
git clone https://github.com/danielmenezsolis/fairly-api.git
cd fairly-api
```

2. Configurar la base de datos:
   - Copia `appsettings.Example.json` a `appsettings.Development.json`
   - Configura tu connection string de Supabase

3. Instalar dependencias:
```bash
dotnet restore
```

4. Ejecutar:
```bash
dotnet run
```

5. Acceder a Swagger:
```
https://localhost:7123/swagger
```

## Estructura de la Base de Datos

Ver el archivo `database-schema.sql` para la estructura completa.
VEr el archivo `datasetbase.json` con ejemplos de creación de registros desde la API

## Endpoints Principales

### Users
- `GET /api/Users` - Listar usuarios
- `POST /api/Users` - Crear usuario
- `GET /api/Users/{id}` - Obtener usuario

### Groups
- `GET /api/Groups` - Listar grupos
- `POST /api/Groups` - Crear grupo
- `POST /api/Groups/{id}/members` - Agregar miembro

### Expenses
- `POST /api/Expenses/equal-split` - Crear gasto con división equitativa
- `POST /api/Expenses/custom-split` - Crear gasto con división personalizada
- `GET /api/Expenses/group/{groupId}/balances` - Ver balances del grupo



## Licencia

MIT
