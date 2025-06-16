# 📚 BitLibro API

**BitLibro** es una aplicación backend RESTful desarrollada en **.NET 8** con **SQL Server** que permite gestionar préstamos de libros mediante un sistema basado en roles: `Admin`, `Employee` y `Client`.

## 🚀 Tecnologías utilizadas

- .NET 8
- Entity Framework Core (EF Core)
- SQL Server
- ASP.NET Core Identity
- JWT (JSON Web Tokens)
- FluentValidation
- Swagger (Swashbuckle)

## 📦 Paquetes NuGet

```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.17" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.17" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.6" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
```

## 🧑‍🤝‍🧑 Roles definidos

- Admin.
- Employee.
- Client.

## 🔐 Base de datos

![Módulo Auth Swagger](capturas/BD.png)

## 🔐 Módulo de Autenticación

Incluye los siguientes endpoints:

- `POST /api/auth/login`: Inicio de sesión con validación usando FluentValidation.
- `POST /api/auth/logout`: Cierre de sesión.
- `GET /api/auth/me`: Obtención de información del usuario autenticado.

![Módulo Auth Swagger](capturas/DOCAUTH.png)

### Digrama de caso de uso
![Módulo Auth Diagrama de casos de uso](capturas/DCAUTH.png)


## 📦 Instalación y configuración

1. **Clona el repositorio:**

```bash
git clone https://github.com/JeanDev-10/APP-BITLIBRO.git
cd APP-BITLIBRO/API/API-BITLIBRO
```

2. **Configura _appsettings.json_:**
   Debes agregar las claves JWT y MailSettings necesarias:

```json
"ConnectionStrings": {
    "DefaultConnection": "Your-default-connection"
  },
  "JWTSettings":{
    "SecretKey":"your-256-bit-secret",
    "Audience":"https://localhost:4200",
    "Issuer":"https://localhost:5188",
    "TokenExpirationInMinutes": 60
  },
```

3. **Ejecuta las migraciones:**

```bash
dotnet ef database update
```

4. **Ejecuta el proyecto:**

```bash
dotnet run
```
---
Acceder a la documentación:
Abrir en navegador: http://localhost:5188/swagger