# Sistema de Gestión de Tareas - .NET 8 Microservices

Este proyecto implementa un Sistema de Gestión de Tareas utilizando una arquitectura de microservicios con .NET 8, Docker, RabbitMQ y otras tecnologías modernas.

**Repositorio:** [https://github.com/Estigma/GestorTareasNet8](https://github.com/Estigma/GestorTareasNet8)

## Descripción General

La empresa XYZ requiere un sistema interno para administrar las tareas de sus equipos. Esta solución proporciona una plataforma backend escalable, mantenible y resiliente para dicha gestión.

El sistema permite:
* Gestionar usuarios (empleados de la empresa).
* Gestionar tareas: creación, descripción, asignación, seguimiento de estado (Backlog, Doing, InReview, Done).
* Actualizar parcialmente el estado de las tareas.
* Asignar tareas a usuarios, con validación de existencia de usuario y estado de la tarea.
* Notificar (mediante logs/mensajes) la asignación de tareas.
* Consultar tareas asignadas a un usuario específico.
* Importar tareas masivamente desde un archivo JSON ubicado en un servidor FTP.

## Arquitectura y Tecnologías

El sistema está diseñado siguiendo una arquitectura de microservicios, priorizando la separación de responsabilidades y la escalabilidad individual de los componentes.

### Diagramas de Arquitectura

Los siguientes diagramas ilustran la arquitectura de la solución:

1.  **Diagrama C4 (Contexto y Contenedores)**: Muestra el sistema en su contexto general, los actores principales y los contenedores (microservicios, bases de datos, etc.) que lo componen y sus interacciones de alto nivel.
    ![Diagrama C4](https://raw.githubusercontent.com/Estigma/GestorTareasNet8/refs/heads/main/Diagramas_ApiGateway/4.%20Diagrama%20de%20secuencia.png)

2.  **Diagrama de Arquitectura General**: Ofrece una vista detallada de los microservicios, el API Gateway, las bases de datos, el broker de mensajes y otras dependencias, así como los flujos de comunicación entre ellos.
    ![Diagrama de Arquitectura](https://raw.githubusercontent.com/Estigma/GestorTareasNet8/refs/heads/main/Diagramas_ApiGateway/2.%20Diagrama%20de%20arquitectura.png)

3.  **Diagrama de Componentes (TaskService)**: Desglosa los componentes internos del microservicio `TaskService`, mostrando sus responsabilidades y cómo interactúan para cumplir con la lógica de negocio relacionada con las tareas.
    ![Diagrama de Componentes TaskService](https://raw.githubusercontent.com/Estigma/GestorTareasNet8/refs/heads/main/Diagramas_ApiGateway/3.%20Diagrama%20de%20componentes%20TaskService.png)

4.  **Diagrama de Secuencia (Asignación de Tarea)**: Ilustra el flujo de interacciones entre los diferentes componentes y servicios cuando se asigna una tarea a un usuario, destacando la comunicación síncrona y asíncrona.
    ![Diagrama de Secuencia](https://raw.githubusercontent.com/Estigma/GestorTareasNet8/refs/heads/main/Diagramas/4.%20Diagrama_ApiGateway%20de%20secuencia.png)

### Componentes Principales

* **ApiGateway**: Punto único de entrada para todas las solicitudes. Desarrollado con [Ocelot](https://github.com/ThreeMammals/Ocelot).
* **ClientService**: Microservicio encargado de la gestión de usuarios.
* **TaskService**: Microservicio responsable de la lógica de negocio de las tareas, incluyendo la importación FTP y la comunicación asíncrona para validaciones y notificaciones.
* **Bases de Datos**: Dos bases de datos SQL Server (una para `ClientService` y otra para `TaskService`) para la persistencia de datos.
* **RabbitMQ**: Broker de mensajes para la comunicación asíncrona entre `TaskService` y `ClientService` (ej. validación de usuarios, notificación de asignación de tareas).
* **Servidor FTP**: Para la funcionalidad de importación masiva de tareas.
* **Jaeger**: Para tracing distribuido.
* **Seq**: Para logging centralizado y estructurado.

### Tecnologías Involucradas

* **.NET 8**: Framework de desarrollo para los microservicios.
* **ASP.NET Core**: Para la creación de APIs RESTful.
* **Entity Framework Core**: ORM para la interacción con las bases de datos SQL Server.
* **Docker & Docker Compose**: Para la contenerización y orquestación de los servicios y dependencias.
* **Ocelot**: Para la implementación del API Gateway.
* **RabbitMQ**: Message broker para comunicación asíncrona.
* **SQL Server**: Sistema de gestión de bases de datos relacionales.
* **Serilog**: Para logging estructurado.
* **OpenTelemetry**: Para la instrumentación y exportación de datos de tracing y métricas (integrado con Jaeger y Prometheus/OTLP).
* **xUnit**: Framework para pruebas unitarias y de integración en el proyecto `TaskService.Test`.
* **Testcontainers**: Para facilitar las pruebas de integración con dependencias como RabbitMQ.
* **FluentAssertions**: Para aserciones más legibles en las pruebas.

### Estructura del Proyecto 

* ***/GestorTareasNet8***
* **|-- ApiGateway/**                   # Proyecto del API Gateway (Ocelot)
* **|-- ClientService/**                # Microservicio de Usuarios
* |   |-- Controllers/
* |   |-- Data/
* |   |-- DTOs/
* |   |-- Models/
* |   |-- Services/                 # Consumidor RabbitMQ para validación
* |   |-- Dockerfile
* |   |-- init-db.sh
* |   |-- init.sql
* |**-- TaskService/**                  # Microservicio de Tareas
* |   |-- Controllers/
* |   |-- Data/
* |   |-- DTOs/
* |   |-- Interfaces/
* |   |-- Models/
* |   |-- Services/                 # Productor RabbitMQ, Validador de Usuario (RPC), Servicio FTP
* |   |-- Dockerfile
* |   |-- init-db.sh
* |   |-- init.sql
* **|-- TaskService.Test/**             # Pruebas para TaskService
* **|-- DIAGRAMAS_ApiGateway_/**                    # Contiene los diagramas de arquitectura
* |   |-- 1. Diagrama C4.png
* |   |-- 2. Diagrama de arquitectura.png
* |   |-- 3. Diagrama de componentes TaskService.png
* |   |-- 4. Diagrama de secuencia.png
* |**-- docker-compose.yml**            # Orquestación de todos los servicios para producción/despliegue
* |**-- GestorTareas.sln**              # Archivo de solución de Visual Studio



## Prerrequisitos

* Docker Desktop instalado y en ejecución.
* .NET 8 SDK (opcional, si se desea compilar o modificar el código fuera de Docker).
* Un cliente GIT para clonar el repositorio.

## Cómo Levantar el Proyecto (Desarrollo Local con Docker)

1.  **Clonar el repositorio:**
    ```bash
    git clone [https://github.com/Estigma/GestorTareasNet8.git](https://github.com/Estigma/GestorTareasNet8.git)
    cd GestorTareasNet8
    ```

2.  **Construir y levantar los contenedores Docker:**
    Desde la raíz del proyecto (donde se encuentra `docker-compose.yml`), ejecutar:
    ```bash
    docker-compose up --build -d
    ```
    Este comando construirá las imágenes de los servicios (si es la primera vez o si hay cambios en los Dockerfiles) y luego iniciará todos los contenedores definidos en `docker-compose.yml` y `docker-compose.override.yml` en modo detached (`-d`).

    Los servicios y sus bases de datos se inicializarán. Los scripts `init-db.sh` y `init.sql` dentro de las carpetas de `ClientService` y `TaskService` son copiados a las imágenes de las bases de datos correspondientes (`db-usuarios`, `db-tareas`) y ejecutados por los entrypoints de dichas imágenes para crear las bases de datos `ClientDB` y `TaskDB` si no existen.

3.  **Verificar que los servicios estén corriendo:**
    Puedes usar `docker ps` para ver los contenedores en ejecución. Deberías ver algo similar a:
    * `apigateway`
    * `clientservice`
    * `taskservice`
    * `db-usuarios`
    * `db-tareas`
    * `rabbitmq`
    * `ftp-server`
    * `jaeger`
    * `seq`

4.  **Acceder a los servicios:**
    * **API Gateway**: `http://localhost:8000`
        * Usuarios: `http://localhost:8000/api/usuarios`
        * Tareas: `http://localhost:8000/api/tareas`
    * **RabbitMQ Management**: `http://localhost:15672` (Credenciales: `admin` / `admin123`)
    * **Seq (Logging)**: `http://localhost:8082` (Interfaz web de Seq, el puerto de ingesta es 5341)
    * **Jaeger (Tracing)**: `http://localhost:16686` (Interfaz web de Jaeger)


5.  **Servidor FTP para importación de tareas (F6):**
    * El servidor FTP está configurado en el `docker-compose.yml` (servicio `ftp-server`).
    * **Host:** `ftp-server` (desde dentro de la red Docker), `localhost` (desde el host si el puerto 21 está mapeado).
    * **Usuario:** `ftpuser`
    * **Contraseña:** `ftppass`
    * **Puerto:** `21`
    * Para probar la importación, necesitarás subir un archivo JSON (con la estructura de `TareaImportada` definida en `TaskService/DTOs/ImportarTareasRequest.cs`) al directorio home del usuario FTP (`/home/ftpuser` dentro del contenedor `ftp-data`). Luego, puedes llamar al endpoint `POST /api/tareas/importar` del `TaskService` (accesible vía API Gateway: `http://localhost:8000/api/tareas/importar`) con el cuerpo `{"nombreArchivo": "tuarchivo.json"}`.

## Endpoints Principales (Vía API Gateway - `http://localhost:8000`)

### ClientService (`/api/usuarios`)

* `GET /api/usuarios`: Obtiene todos los usuarios.
* `GET /api/usuarios/{id}`: Obtiene un usuario por ID.
* `POST /api/usuarios`: Crea un nuevo usuario.
* `PUT /api/usuarios/{id}`: Actualiza un usuario existente.
* `DELETE /api/usuarios/{id}`: Elimina (soft delete) un usuario.

### TaskService (`/api/tareas`)

* `GET /api/tareas`: Obtiene todas las tareas.
* `GET /api/tareas/{id}`: Obtiene una tarea por ID.
* `POST /api/tareas`: Crea una nueva tarea.
* `PUT /api/tareas/{id}`: Actualiza una tarea existente.
* `PATCH /api/tareas/{id}`: Actualiza parcialmente una tarea (ej. estado, tiempo de desarrollo).
    * Campos permitidos para PATCH: `estadoTarea`, `tiempoDesarrollo`.
* `DELETE /api/tareas/{id}`: Elimina (soft delete) una tarea.
* `POST /api/tareas/{id}/asignar`: Asigna una tarea a un usuario.
    * Body: `{"usuarioId": X}`
* `GET /api/tareas/usuario/{usuarioId}`: Obtiene todas las tareas asignadas a un usuario específico.
* `POST /api/tareas/importar`: Importa tareas desde un archivo JSON en el servidor FTP.
    * Body: `{"nombreArchivo": "nombre_del_archivo.json"}`

## Ejecución de Pruebas (TaskService.Test)

Si deseas ejecutar las pruebas (requiere el SDK de .NET 8):
1.  Navega a la carpeta `TaskService.Test`.
2.  Ejecuta el comando:
    ```bash
    dotnet test
    ```
    Las pruebas de integración en `TareasConRabbitTests.cs` utilizan `Testcontainers` para levantar una instancia de RabbitMQ automáticamente.

## Para Detener la Aplicación

Para detener y eliminar los contenedores, ejecuta:
```bash
docker-compose down
```
Si también deseas eliminar los volúmenes (¡esto borrará los datos de las bases de datos y FTP!), usa:

```bash
docker-compose down -v
```
