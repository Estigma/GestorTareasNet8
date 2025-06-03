USE [TaskDB]
GO

CREATE TABLE [dbo].[Tareas](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CodigoTarea] [nvarchar](max) NOT NULL,
	[Titulo] [nvarchar](max) NOT NULL,
	[Descripcion] [nvarchar](max) NOT NULL,
	[CriteriosAceptacion] [nvarchar](max) NOT NULL,
	[FechaInicio] [datetime2](7) NULL,
	[FechaFinalizacion] [datetime2](7) NULL,
	[TiempoDesarrollo] [int] NOT NULL,
	[EstadoTarea] [nvarchar](max) NOT NULL,
	[Estado] [bit] NOT NULL,
	[UsuarioId] [int] NOT NULL,
 CONSTRAINT [PK_Tareas] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Tareas] ADD  DEFAULT ((0)) FOR [TiempoDesarrollo]
GO

ALTER TABLE [dbo].[Tareas] ADD  DEFAULT (N'Backlog') FOR [EstadoTarea]
GO

ALTER TABLE [dbo].[Tareas]  WITH CHECK ADD  CONSTRAINT [CK_Tarea_EstadoTarea] CHECK  (([EstadoTarea]='Done' OR [EstadoTarea]='InReview' OR [EstadoTarea]='Doing' OR [EstadoTarea]='Backlog'))
GO

ALTER TABLE [dbo].[Tareas] CHECK CONSTRAINT [CK_Tarea_EstadoTarea]
GO


