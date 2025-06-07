# Sistema de Gestión de Tareas - .NET 8 Microservices en Kubernetes

Este proyecto implementa un Sistema de Gestión de Tareas utilizando una arquitectura de microservicios con .NET 8, Docker, Kubernetes y RabbitMQ.

**Repositorio:** [https://github.com/Estigma/GestorTareasNet8](https://github.com/Estigma/GestorTareasNet8)

## Descripción General

Esta solución proporciona una plataforma backend escalable, mantenible y resiliente para la gestión de tareas internas de una empresa. El sistema ha sido migrado de una orquestación con Docker Compose a un despliegue nativo en Kubernetes, utilizando Nginx Ingress Controller como punto de entrada único.

El sistema permite:
* Gestionar usuarios (empleados de la empresa).
* Gestionar tareas: creación, descripción, asignación, seguimiento de estado (Backlog, Doing, InReview, Done).
* Actualizar parcialmente el estado de las tareas.
* Asignar tareas a usuarios, con validación de existencia y estado de la tarea.
* Notificar (mediante mensajes asíncronos) la asignación de tareas.
* Consultar tareas asignadas a un usuario específico.
* Importar tareas masivamente desde un archivo JSON ubicado en un servidor FTP.

## Arquitectura y Tecnologías

El sistema está diseñado siguiendo una arquitectura de microservicios y desplegado en Kubernetes para garantizar escalabilidad y resiliencia.

### Diagramas de Arquitectura

1.  **Diagrama C4 (Contexto y Contenedores)**: Muestra el sistema en su contexto general, los actores principales y los contenedores (microservicios, bases de datos, etc.) que lo componen.
    ![Diagrama C4](https://raw.githubusercontent.com/Estigma/GestorTareasNet8/main/Diagramas/1.%20Diagrama%20C4.png)

2.  **Diagrama de Arquitectura General**: Ofrece una vista detallada de los microservicios, bases de datos, y el broker de mensajes.
    ![Diagrama de Arquitectura](https://raw.githubusercontent.com/Estigma/GestorTareasNet8/main/Diagramas/2.%20Diagrama%20de%20arquitectura.png)

3.  **Diagrama de Componentes (TaskService)**: Desglosa los componentes internos del microservicio `TaskService`.
    ![Diagrama de Componentes TaskService](https://raw.githubusercontent.com/Estigma/GestorTareasNet8/main/Diagramas/3.%20Diagrama%20de%20componentes%20TaskService.png)

4.  **Diagrama de Secuencia (Asignación de Tarea)**: Ilustra el flujo de interacciones cuando se asigna una tarea a un usuario.
    ![Diagrama de Secuencia](https://raw.githubusercontent.com/Estigma/GestorTareasNet8/main/Diagramas/4.%20Diagrama%20de%20secuencia.png)

### Componentes Principales

* **Nginx Ingress Controller**: Actúa como punto de entrada único para todas las solicitudes, gestionando el enrutamiento y balanceo de carga L7 hacia los microservicios.
* **ClientService**: Microservicio encargado de la gestión de usuarios.
* **TaskService**: Microservicio responsable de la lógica de negocio de las tareas.
* **Bases de Datos**: Dos bases de datos SQL Server persistentes para `ClientService` y `TaskService`.
* **RabbitMQ**: Broker de mensajes para la comunicación asíncrona entre servicios.
* **Servidor FTP**: Para la funcionalidad de importación masiva de tareas.
* **Jaeger**: Para tracing distribuido.
* **Seq**: Para logging centralizado y estructurado.

### Tecnologías Involucradas

* **.NET 8** y **ASP.NET Core**
* **Entity Framework Core**
* **Docker**: Para la contenerización de las aplicaciones.
* **Kubernetes (K8s)**: Para la orquestación, escalado y gestión de los contenedores.
* **Nginx Ingress Controller**: Para el enrutamiento de entrada.
* **RabbitMQ**: Message broker.
* **SQL Server**
* **Serilog** y **OpenTelemetry** (con Jaeger y Seq) para observabilidad.
* **xUnit**, **Moq**, **Testcontainers**: Para pruebas unitarias y de integración.

## Estructura del Proyecto


* ***/GestorTareasNet8***
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
* **|-- DIAGRAMAS/**                    # Contiene los diagramas de arquitectura
* |   |-- 1. Diagrama C4.png
* |   |-- 2. Diagrama de arquitectura.png
* |   |-- 3. Diagrama de componentes TaskService.png
* |   |-- 4. Diagrama de secuencia.png
* **|-- k8s-manifests/**                # Manifiestos YAML para el despliegue en Kubernetes
* |   |-- namespace.yaml
* |   |-- pvc-db-usuarios.yaml        # Persistent Volume Claims para las bases de datos y otros servicios
* |   |-- pvc-db-tareas.yaml 
* |   |-- pvc-rabbitmq.yaml
* |   |-- pvc-ftp.yaml
* |   |-- pvc-seq.yaml
* |   |-- deployment-db-usuarios.yaml  # Despliegue de las bases de datos y otros servicios
* |   |-- deployment-db-tareas.yaml
* |   |-- deployment-rabbitmq.yaml
* |   |-- deployment-ftp-server.yaml
* |   |-- deployment-jaeger.yaml
* |   |-- deployment-seq.yaml
* |   |-- deployment-clientservice.yaml
* |   |-- deployment-taskservice.yaml
* |   |-- service-db-usuarios.yaml      # Servicios para acceder a las bases de datos y otros servicios
* |   |-- service-db-tareas.yaml
* |   |-- service-rabbitmq.yaml
* |   |-- service-ftp-server.yaml
* |   |-- service-jaeger.yaml
* |   |-- service-seq.yaml
* |   |-- service-clientservice.yaml
* |   |-- service-taskservice.yaml
* |   |-- hpa-clientservice.yaml        # Horizontal Pod Autoscaler para ClientService
* |   |-- hpa-taskservice.yaml
* |   |-- ingress.yaml                   # Configuración de Ingress para exponer los servicios
* |**-- GestorTareas.sln**              # Archivo de solución de Visual Studio


## Prerrequisitos

* **Docker Desktop** instalado y en ejecución.
* **Kubernetes habilitado** en la configuración de Docker Desktop.
* **Nginx Ingress Controller** instalado y funcionando en el clúster de Docker Desktop.
* **Metrics Server** instalado y funcionando en el clúster de Docker Desktop.
* **`.NET 8 SDK`** (opcional, para compilar localmente o ejecutar pruebas).
* **`kubectl` CLI** configurado para apuntar al clúster de Docker Desktop.
* Un cliente GIT para clonar el repositorio.

## Cómo Levantar el Proyecto (Kubernetes en Docker Desktop)

1.  **Clonar el repositorio:**
    ```bash
    git clone [https://github.com/Estigma/GestorTareasNet8.git](https://github.com/Estigma/GestorTareasNet8.git)
    cd GestorTareasNet8
    ```

2.  **Construir y Publicar Imágenes Docker:**
    Asegúrate de que tus imágenes Docker estén construidas y disponibles para tu clúster de Kubernetes. Puedes subirlas a un registro como Docker Hub o construirlas localmente. Si las construyes localmente, asegúrate que la `imagePullPolicy` en tus Deployments esté como `IfNotPresent` o `Never`.

    ```bash
    # Reemplaza 'estigma' con tu Docker ID si vas a subir a Docker Hub
    docker build -t estigma/clientservice:latest -f ClientService/Dockerfile .
    docker build -t estigma/taskservice:latest -f TaskService/Dockerfile .
    
    # Si usas un registro remoto, no olvides hacer push:
    # docker push estigma/clientservice:latest
    # docker push estigma/taskservice:latest
    ```
    *Nota: Asegúrate de que los nombres de las imágenes en los archivos `deployment-*.yaml` coincidan con los que has construido.*

3.  **Desplegar los Recursos en Kubernetes:**
    Navega a la carpeta que contiene los manifiestos YAML (ej. `k8s-manifests/`) y aplica los archivos en el siguiente orden. Se recomienda usar un namespace para mantener los recursos organizados.

    ```bash
    # 1. Crear el Namespace
    kubectl apply -f namespace.yaml
    
    # 2. Crear los Persistent Volume Claims (PVCs) para el almacenamiento
    kubectl apply -f pvc-db-usuarios.yaml -f pvc-db-tareas.yaml -f pvc-rabbitmq.yaml -f pvc-ftp.yaml -f pvc-seq.yaml -n gestor-tareas

    # 3. Desplegar dependencias (BBDD, RabbitMQ, etc.) y sus Services
    kubectl apply -f deployment-db-usuarios.yaml -f service-db-usuarios.yaml -n gestor-tareas
    kubectl apply -f deployment-db-tareas.yaml -f service-db-tareas.yaml -n gestor-tareas
    kubectl apply -f deployment-rabbitmq.yaml -f service-rabbitmq.yaml -n gestor-tareas
    kubectl apply -f deployment-ftp-server.yaml -f service-ftp-server.yaml -n gestor-tareas
    kubectl apply -f deployment-jaeger.yaml -f service-jaeger.yaml -n gestor-tareas
    kubectl apply -f deployment-seq.yaml -f service-seq.yaml -n gestor-tareas
    
    # Espera a que estos pods estén en estado 'Running'
    kubectl get pods -n gestor-tareas -w

    # 4. Desplegar tus microservicios
    kubectl apply -f deployment-clientservice.yaml -f service-clientservice.yaml -n gestor-tareas
    kubectl apply -f deployment-taskservice.yaml -f service-taskservice.yaml -n gestor-tareas
    
    # 5. Desplegar los Horizontal Pod Autoscalers (HPAs)
    kubectl apply -f hpa-clientservice.yaml -f hpa-taskservice.yaml -n gestor-tareas
    
    # 6. Desplegar el Ingress para exponer los servicios
    kubectl apply -f ingress.yaml -n gestor-tareas
    ```

4.  **Verificar el Despliegue:**
    Revisa que todos los pods estén corriendo correctamente en el namespace `gestor-tareas`:
    ```bash
    kubectl get pods -n gestor-tareas
    ```

## Acceder a los Servicios y Herramientas

* **API Principal**: El Ingress Controller expone los servicios en `localhost`.
    * Usuarios: `http://localhost/api/usuarios`
    * Tareas: `http://localhost/api/tareas`

* **RabbitMQ Management**: `http://localhost:15672` (Credenciales: `admin` / `admin123`).

* **Seq (Logging)**: http://localhost:30672`.

* **Jaeger (Tracing)**: `http://localhost:30686`.

* **Servidor FTP**: `ftp://127.0.0.1:30021/`
    * **Host:** `localhost`    
    * **Usuario:** `ftpuser`
    * **Contraseña:** `ftppass`

## Para Detener y Limpiar el Entorno

Para eliminar todos los recursos creados para este proyecto, la forma más sencilla es borrar el namespace:
```bash
kubectl delete namespace gestor-tareas

¡Advertencia! Esto eliminará todos los Deployments, Services, Pods, PVCs e Ingresses dentro de ese namespace. Para eliminar también los datos persistentes, es posible que necesites borrar los PersistentVolumes (PVs) manualmente.

# Opcional: listar y borrar PVs si quedaron en estado 'Released'
kubectl get pv
kubectl delete pv <nombre-del-pv>
