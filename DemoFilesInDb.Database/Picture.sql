CREATE TABLE [dbo].[Picture]
(
	[Id] INT NOT NULL IDENTITY,
	[Name] VARCHAR(256) NOT NULL,
	[Type] nvarchar(50) NOT NULL,
	[Content] VARBINARY(MAX) NOT NULL,
    CONSTRAINT [PK_Picture] PRIMARY KEY ([Id])
)
