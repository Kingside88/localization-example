-- Schema
CREATE SCHEMA [Local]

-- Table: Language
CREATE TABLE [Local].[Language]
(
	[LanguageId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [Code2] CHAR(2) NOT NULL
)
GO

-- Table: Translation
CREATE TABLE [Local].[Translation]
(
	[TranslationId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [LanguageId] INT NOT NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [Key] NVARCHAR(255) NULL,
    [Text] NVARCHAR(MAX) NULL,

    CONSTRAINT [FK_Translation_LanguageId] FOREIGN KEY ([LanguageId]) REFERENCES [Local].[Language] ([LanguageId]), 
)
GO

-- Stored Procedure Save Translation
CREATE PROCEDURE [Local].[spTranslation_Command_Save]
	@Code2 CHAR(2)
      , @Key NVARCHAR(255)
      , @Text NVARCHAR(MAX)
AS
SET NOCOUNT ON;

DECLARE @LanguageId INT = (SELECT [LanguageId] FROM [Local].[Language] WHERE [Code2] = @Code2)

IF NOT EXISTS (SELECT 1 FROM [Local].[Translation] WHERE [LanguageId] = @LanguageId AND [Key] = @Key)
BEGIN
    INSERT INTO [Local].[Translation] (
    [LanguageId]
	, [Key]
    , [Text]
 )
    VALUES (
    @LanguageId
    , @Key
    , @Text
)
END

ELSE 
BEGIN
    UPDATE [Local].[Translation] SET
        [Text] = @Text
    WHERE [LanguageId] = @LanguageId AND [Key] = @Key
END
GO

-- Stored Procedure Save with Translation. Practical for forcing each language to have a translation.
CREATE PROCEDURE [Local].[spTranslation_Command_SaveWithTranslation]
    @Key NVARCHAR(255)
    , @Text_de NVARCHAR(MAX)
    , @Text_en NVARCHAR(MAX)
AS
SET NOCOUNT ON;

DECLARE @LanguageId_de INT = (SELECT [LanguageId] FROM [Local].[Language] WHERE [Code2] = 'de')
DECLARE @LanguageId_en INT = (SELECT [LanguageId] FROM [Local].[Language] WHERE [Code2] = 'en')

IF NOT EXISTS (SELECT 1 FROM [Local].[Translation] WHERE [LanguageId] = @LanguageId_de AND [Key] = @Key)
BEGIN
    INSERT INTO [Local].[Translation] (
        [LanguageId]
        , [Key]
        , [Text]
    )
    VALUES (
        @LanguageId_de
        , @Key
        , @Text_de
    )
END
ELSE 
BEGIN
    UPDATE [Local].[Translation] SET
        [Text] = @Text_de
    WHERE [LanguageId] = @LanguageId_de AND [Key] = @Key
END

IF NOT EXISTS (SELECT 1 FROM [Local].[Translation] WHERE [LanguageId] = @LanguageId_en AND [Key] = @Key)
BEGIN
    INSERT INTO [Local].[Translation] (
        [LanguageId]
        , [Key]
        , [Text]
    )
    VALUES (
        @LanguageId_en
        , @Key
        , @Text_en
    )
END
ELSE 
BEGIN
    UPDATE [Local].[Translation] SET
        [Text] = @Text_en
    WHERE [LanguageId] = @LanguageId_en AND [Key] = @Key
END
GO

EXEC [Local].[spTranslation_Command_Save] 'test', 'Hello from EN', 'Hallo aus DE'

