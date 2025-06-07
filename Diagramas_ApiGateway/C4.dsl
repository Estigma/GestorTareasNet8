    workspace "Sistema de Gestión de Tareas XYZ" "Descripción del sistema de gestión de tareas para la empresa XYZ." {
    
        model {
            # Actores (Personas)
            user = person "Empleado de XYZ" "Usuario del sistema de gestión de tareas." "Actor"
    
            # Sistemas Externos
            ftpServer = softwareSystem "Servidor FTP" "Almacena archivos JSON de tareas para importación." "Sistema Externo" {
                tags "Externo"
            }
    
            # Sistema Principal
            gestorTareasSystem = softwareSystem "Sistema de Gestión de Tareas XYZ" "Plataforma interna para administrar tareas del equipo." {
                tags "SistemaPrincipal"
    
                # --- Contenedores dentro del Sistema de Gestión de Tareas XYZ ---
                apiGateway = container "API Gateway" "Gestiona y enruta las peticiones externas a los microservicios internos." "Ocelot, ASP.NET Core" "Gateway" {
                    tags "Gateway"
                }
    
                clientService = container "ClientService" "Microservicio para la gestión de usuarios (clientes)." "ASP.NET Core Web API, Entity Framework Core" "Microservicio" {
                    tags "Microservicio"
                    
                    # Componentes del ClientService
                    usuarioValidationConsumer = component "UsuarioValidationConsumer" "Consume mensajes de RabbitMQ para validar la existencia de usuarios." "RabbitMQ Consumer, BackgroundService" "Consumidor de Mensajes"
                    clientRepo = component "UsuarioRepository" "Gestiona el acceso a datos de los usuarios." "Entity Framework Core" "Repositorio"
                }
    
                taskService = container "TaskService" "Microservicio para la gestión de tareas." "ASP.NET Core Web API, Entity Framework Core" "Microservicio" {
                    tags "Microservicio"
                    
                    # Componentes del TaskService
                    tareasController = component "TareasController" "Expone los endpoints HTTP para la gestión de tareas." "ASP.NET Core MVC Controller" "API Controller"
                    usuarioValidatorRabbit = component "UsuarioValidatorRabbitMQ" "Cliente RPC para validar usuarios vía RabbitMQ." "RabbitMQ Client" "Servicio de Validación"
                    rabbitMQProducer = component "RabbitMQProducer" "Publica mensajes de notificación de asignación de tareas." "RabbitMQ Client" "Productor de Mensajes"
                    ftpServiceComponent = component "FtpService" "Interactúa con el servidor FTP para descargar archivos." "FTP Client" "Servicio FTP"
                    tareaRepository = component "TareaRepository" "Gestiona el acceso a datos de las tareas." "Entity Framework Core" "Repositorio"
                    taskDbContext = component "AppDbContext (Task)" "Contexto de base de datos para tareas." "Entity Framework Core" "DbContext"
                }
    
                userDB = container "Database - Usuarios" "Base de datos relacional para almacenar información de usuarios." "Microsoft SQL Server" "Base de Datos" {
                    tags "Database"
                }
    
                taskDB = container "Database - Tareas" "Base de datos relacional para almacenar información de tareas." "Microsoft SQL Server" "Base de Datos" {
                    tags "Database"
                }
    
                messageBroker = container "RabbitMQ" "Broker de mensajes para comunicación asíncrona entre servicios." "RabbitMQ" "Broker de Mensajes" {
                    tags "MessageBroker"
                }
            }
    
            # --- Relaciones del Modelo ---
    
            # Nivel de Contexto
            user -> gestorTareasSystem "Utiliza" "HTTP/HTTPS"
            gestorTareasSystem -> ftpServer "Lee archivos de tareas desde" "FTP"
    
            # Nivel de Contenedores (dentro de gestorTareasSystem)
            user -> apiGateway "Realiza peticiones API" "HTTP/HTTPS"
    
            apiGateway -> clientService "Enruta a /api/usuarios/*" "HTTP"
            apiGateway -> taskService "Enruta a /api/tareas/*" "HTTP"
    
            # Add explicit relationship between API Gateway and TareasController
            apiGateway -> tareasController "Enruta peticiones de tareas" "HTTP"
    
            clientService -> userDB "Lee/Escribe datos de usuarios" "EF Core, TCP/IP"
            clientService -> messageBroker "Consume/Publica (validación usuario)" "AMQP"
    
            taskService -> taskDB "Lee/Escribe datos de tareas" "EF Core, TCP/IP"
            taskService -> messageBroker "Publica/Consume (validación, notificación)" "AMQP"
            taskService -> ftpServer "Descarga archivos de tareas" "FTP"
    
            # Relaciones de Componentes (ClientService)
            usuarioValidationConsumer -> clientRepo "Verifica existencia de usuario"
            usuarioValidationConsumer -> messageBroker "Responde a la validación" "AMQP (Reply Queue)"
    
            # Relaciones de Componentes (TaskService)
            tareasController -> tareaRepository "Accede a datos de tareas"
            tareasController -> usuarioValidatorRabbit "Valida existencia de usuario"
            tareasController -> rabbitMQProducer "Envía notificación de asignación"
            tareasController -> ftpServiceComponent "Solicita descarga de archivo FTP"
    
            tareaRepository -> taskDbContext "Usa"
            usuarioValidatorRabbit -> messageBroker "Envía solicitud de validación" "AMQP (RPC)"
            rabbitMQProducer -> messageBroker "Publica mensaje de asignación" "AMQP (Fanout Exchange)"
            ftpServiceComponent -> ftpServer "Descarga archivo" "FTP"
        }
    
        views {
            # --- 1. Diagrama C4 - Nivel 1: Contexto del Sistema ---
            systemContext gestorTareasSystem "SystemContext" "Diagrama de contexto del Sistema de Gestión de Tareas XYZ." {
                include *
                autoLayout lr
            }
    
            # --- 2. Diagrama C4 - Nivel 2: Contenedores ---
            container gestorTareasSystem "Containers" "Diagrama de contenedores del Sistema de Gestión de Tareas XYZ." {
                include *
                autoLayout lr
            }
    
            # --- 3. Diagrama de Componentes (TaskService) ---
            component taskService "TaskServiceComponents" "Diagrama de componentes internos del TaskService." {
                include *
                autoLayout lr
            }
    
            # --- 4. Diagrama de Secuencia (Servicio de Asignación de Tarea - F3) ---
            dynamic taskService "TaskAssignmentSequence" "Secuencia de asignación de una tarea a un usuario." {
                # Actores y Contenedores/Componentes participantes
                user -> apiGateway "POST /api/tareas/{id}/asignar (UsuarioId)"
                apiGateway -> tareasController "Reenvía petición POST"
                tareasController -> tareaRepository "GetTareaByIdAsync(id)"
                tareaRepository -> tareasController "Retorna tarea"
                
                tareasController -> usuarioValidatorRabbit "UsuarioExisteAsync(UsuarioId)"
                usuarioValidatorRabbit -> messageBroker "Publica msg validación (Req: UsuarioValidationRequest)"
                messageBroker -> usuarioValidationConsumer "Entrega msg validación"
                usuarioValidationConsumer -> clientRepo "VerificarUsuarioExisteAsync(UsuarioId)"
                clientRepo -> usuarioValidationConsumer "Retorna bool existe"
                usuarioValidationConsumer -> messageBroker "Publica msg respuesta (Resp: UsuarioValidationResponse)"
                messageBroker -> usuarioValidatorRabbit "Entrega msg respuesta"
                usuarioValidatorRabbit -> tareasController "Retorna bool usuarioExiste"
                
                tareasController -> tareaRepository "UpdateTareaAsync(tarea con UsuarioId)"
                tareaRepository -> tareasController "Confirmación de actualización"
    
                tareasController -> rabbitMQProducer "SendMessage(AsignacionTareaMessage)"
                rabbitMQProducer -> messageBroker "Publica msg notificación (AsignacionTareaMessage)"
    
                tareasController -> apiGateway "Respuesta 204 NoContent"
                apiGateway -> user "Respuesta 204 NoContent"
            }
    
            # --- Estilos ---
            styles {
                element "Actor" {
                    background #cccccc
                    color #000000
                    shape Person
                }
                element "SistemaPrincipal" {
                    background #ffffff
                }
                element "Gateway" {
                    background #0a437c
                    color #ffffff
                    shape Component
                }
                element "Microservicio" {
                    background #1168bd
                    color #ffffff
                    shape Component
                }
                element "Base de Datos" {
                    background #A5B5C5
                    color #000000
                    shape Cylinder
                }
                element "Broker de Mensajes" {
                    background #ff6600
                    color #ffffff
                    shape Component
                }
                element "API Controller" {
                    background #2596be
                    color #ffffff
                    shape Component
                }
                element "Servicio de Validación" {
                    background #78c4d4
                    color #000000
                    shape Component
                }
                element "Productor de Mensajes" {
                    background #78c4d4
                    color #000000
                    shape Component
                }
                element "Servicio FTP" {
                    background #78c4d4
                    color #000000
                    shape Component
                }
                element "Consumidor de Mensajes" {
                    background #78c4d4
                    color #000000
                    shape Component
                }
                element "Repositorio" {
                    background #90ee90
                    color #000000
                    shape Component
                }
                element "DbContext" {
                    background #add8e6
                    color #000000
                    shape Component
                }
            }
        }
    }