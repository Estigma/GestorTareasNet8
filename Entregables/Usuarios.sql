USE [ClientDB]
GO

CREATE TABLE [dbo].[Usuarios](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Identificacion] [nvarchar](max) NOT NULL,
	[Nombres] [nvarchar](max) NOT NULL,
	[Apellidos] [nvarchar](max) NOT NULL,
	[Edad] [int] NOT NULL,
	[Cargo] [nvarchar](max) NOT NULL,
	[Estado] [bit] NOT NULL,
 CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


