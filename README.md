# KRO-Training
Desarrollo del proyecto KRÒ Training Performance
KRÒ Training Performance

Plataforma web para la gestión y administración integral de gimnasios.

Estado del proyecto: 70 % de desarrollo

Descripción del proyecto

KRÒ Training Performance es un proyecto de desarrollo de software enfocado en optimizar y centralizar los procesos administrativos y operativos del gimnasio KRÒ Training.

La plataforma busca sustituir los procesos manuales basados en hojas de cálculo, documentos PDF y aplicaciones de mensajería por una solución integrada que permita administrar clientes, membresías, pagos, asistencia, rutinas de entrenamiento y seguimiento del progreso físico.

El sistema se desarrolla mediante una arquitectura modular que facilita el mantenimiento, la escalabilidad y la incorporación de nuevas funcionalidades.

Problemática

Conforme aumenta la cantidad de clientes de un gimnasio, administrar la información mediante documentos y herramientas independientes se vuelve cada vez más complejo.

Entre las principales dificultades identificadas se encuentran:

* Información distribuida en diferentes plataformas.
* Duplicidad e inconsistencias en los registros.
* Procesos administrativos que requieren demasiado tiempo.
* Dificultad para controlar membresías, pagos y asistencia.
* Limitaciones para consultar el historial de los clientes.
* Dificultad para dar seguimiento al progreso físico de los usuarios.

KRÒ Training Performance busca solucionar estas necesidades mediante la centralización de la información en una única plataforma.

Funcionalidades principales

El sistema contempla diferentes áreas funcionales:

* Gestión de clientes y personal.
* Administración de membresías y pagos.
* Control de asistencia.
* Gestión de rutinas de entrenamiento.
* Seguimiento del progreso físico.
* Información multimedia sobre ejercicios.
* Generación de reportes e indicadores de rendimiento.
* Administración de usuarios y permisos según roles.

Como parte de las futuras ampliaciones, se contemplan funcionalidades de inteligencia artificial para la recomendación de rutinas personalizadas y el análisis de retención de clientes.

Roles de usuario

La plataforma contempla cinco perfiles de usuario:

Rol	Responsabilidad
Administrador	Configuración del sistema y gestión de usuarios.
Recepcionista	Registro de clientes, membresías, pagos y asistencia.
Entrenador	Administración de rutinas y seguimiento físico.
Cliente	Consulta de información personal, rutinas y progreso.
Gerencia	Consulta de reportes, indicadores y métricas operativas.

Tecnologías utilizadas

Área	Tecnologías
Backend	C#, ASP.NET Core MVC, .NET 9
Frontend	Razor Views, HTML5, CSS, Bootstrap 5
Base de datos	Microsoft SQL Server 2022
ORM	Entity Framework Core
Autenticación	ASP.NET Core Identity
Entorno de desarrollo	Visual Studio 2022
Control de versiones	Git y GitHub

Arquitectura del sistema

La aplicación está diseñada bajo una arquitectura de tres capas, implementando el patrón de diseño Modelo-Vista-Controlador (MVC).

La solución separa sus responsabilidades en tres capas principales:

1. Capa de presentación: administra la interfaz de usuario y la interacción con la aplicación mediante Razor Views, HTML, CSS y Bootstrap.
2. Capa de lógica de negocio: contiene las reglas, validaciones y procesos operativos del sistema.
3. Capa de acceso a datos: administra las operaciones con la base de datos mediante Entity Framework Core y SQL Server.

Esta separación permite mantener una estructura organizada, facilita el mantenimiento y permite ampliar las funcionalidades del sistema.

Modelo de base de datos

La base de datos utiliza un modelo relacional para organizar la información de la plataforma, incluyendo usuarios, clientes, entrenadores, membresías, pagos, rutinas, asistencia y evaluaciones físicas.

Capturas de pantalla

En esta sección se incorporarán capturas de las principales interfaces de la aplicación conforme avance el desarrollo.


Instalación y ejecución

Requisitos previos

Para ejecutar el proyecto localmente se contempla el siguiente entorno:

* Windows.
* Visual Studio 2022.
* .NET 9 SDK.
* Microsoft SQL Server 2022.
* SQL Server Management Studio (SSMS).

Pasos de instalación

1. Clonar el repositorio:

git clone https://github.com/JohanEEG/KRO-Training.git

2. Abrir la solución en Visual Studio 2022.
3. Configurar la cadena de conexión a SQL Server en appsettings.json, según el entorno local.
4. Restaurar las dependencias del proyecto.
5. Configurar la base de datos mediante los scripts SQL o las migraciones de Entity Framework Core, según la configuración del repositorio.
6. Compilar y ejecutar la aplicación.

Las credenciales de base de datos y demás valores sensibles deben mantenerse fuera del control de versiones.

Estado del proyecto

70 % de desarrollo — En progreso.

El proyecto cuenta con un avance significativo en su arquitectura, diseño de base de datos e implementación de la aplicación.

Actualmente se encuentra en desarrollo, con funcionalidades adicionales, pruebas y mejoras pendientes.

Repositorio

GitHub – KRÒ Training Performance
