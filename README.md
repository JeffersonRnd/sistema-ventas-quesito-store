# Quesito Store - Sistema de Ventas

Sistema web para la gestión de ventas de **Quesito Store**, desarrollado con ASP.NET Core y SQL Server.

---

## Requisitos

Para ejecutar el proyecto de forma local necesitas tener instalado:
- Visual Studio
- .NET / ASP.NET Core correspondiente a la versión del proyecto
- SQL Server
- SQL Server Management Studio (SSMS)
- Un navegador web (Chrome, Edge, Firefox, etc.)

---

# Instalación

## 1. Descargar o clonar el repositorio

Puedes obtener el proyecto de dos formas.

### Opción 1: Clonar el repositorio

Abre una terminal (Git Bash, PowerShell o CMD) y ejecuta:

```bash
git clone https://github.com/JeffersonRnd/sistema-ventas-quesito-store.git
```

Después, ingresa a la carpeta del proyecto:

```bash
cd sistema-ventas-quesito-store
```

### Opción 2: Descargar como ZIP

También puedes descargar el proyecto directamente desde GitHub:

**Code → Download ZIP**

Después:

1. Descomprime el archivo `.zip`.
2. Ingresa a la carpeta del proyecto.
3. Busca el archivo de solución:

```text
sistema-ventas-quesito-store.sln
```

---

# 2. Abrir el proyecto

Abre el archivo:

```text
sistema-ventas-quesito-store.sln
```

utilizando **Visual Studio**.

Espera a que Visual Studio cargue el proyecto y restaure las dependencias necesarias.

---

# 3. Configurar SQL Server

Para que el sistema funcione correctamente, es necesario tener una instancia de **SQL Server** disponible.

Abre **SQL Server Management Studio (SSMS)** y conéctate a tu servidor.

# 4. Configurar la conexión a SQL Server

Dentro del proyecto, busca el archivo:

```text
appsettings.json
```

Ubica la sección:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "CadenaSQL": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=QuesitoStore_db;TrustServerCertificate=True;Trusted_Connection=True;"
  }
}
```

Si tu servidor o instancia de SQL Server es diferente, debes modificar el valor de:

```text
Data Source
```

Por ejemplo:

```text
Data Source=localhost\SQLEXPRESS;
```

También puedes modificar el nombre de la base de datos mediante:

```text
Initial Catalog
```

Por ejemplo:

```text
Initial Catalog=QuesitoStore_db;
```

### Ejemplo

Si tu instancia de SQL Server es:

```text
localhost\SQLEXPRESS
```

la conexión quedaría:

```json
"ConnectionStrings": {
  "CadenaSQL": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=QuesitoStore_db;TrustServerCertificate=True;Trusted_Connection=True;"
}
```

> **Nota:** La instancia indicada en `Data Source` debe coincidir con la instancia de SQL Server instalada en tu equipo.

---

# 5. Credenciales de prueba

Dentro del repositorio se encuentra el archivo:

```text
credenciales para las pruebas.txt
```

Este archivo contiene las credenciales de los diferentes usuarios/roles disponibles para realizar las pruebas del sistema.

Utiliza las credenciales indicadas en dicho archivo para iniciar sesión.

> **Importante:** No modifiques las credenciales de prueba a menos que sea necesario para realizar alguna prueba específica.

---

# 6. Ejecutar el proyecto

Una vez configurada la base de datos y la cadena de conexión:
- Ejecuta el proyecto utilizando el botón **HTTP** o la configuración disponible en Visual Studio.

Al ejecutarlo, Visual Studio abrirá el sistema en una dirección similar a:

```text
https://localhost:xxxx
```

> El puerto puede variar dependiendo de la configuración de cada equipo.

---

# 7. Pruebas con diferentes usuarios

Actualmente el sistema se ejecuta de forma **local** y todavía no se encuentra desplegado en un servidor público.

Para probar correctamente los diferentes usuarios y roles del sistema, se recomienda abrir varias sesiones simultáneamente.

Por ejemplo:

| Sesión | Usuario |
|---|---|
| Navegador 1 | Usuario/Rol 1 |
| Navegador 2 | Usuario/Rol 2 |
| Navegador 3 | Usuario/Rol 3 |
| Navegador 4 | Usuario/Rol 4 |

Puedes utilizar diferentes navegadores o sesiones independientes:

- Google Chrome
- Microsoft Edge
- Mozilla Firefox
- Ventana de incógnito
- Ventana normal

### Ejemplo

Puedes realizar las pruebas utilizando:

```text
Chrome normal       → Usuario 1
Chrome incógnito    → Usuario 2
Microsoft Edge      → Usuario 3
Firefox             → Usuario 4
```

Esto permite mantener diferentes sesiones iniciadas al mismo tiempo y comprobar el comportamiento del sistema dependiendo del usuario o rol.
