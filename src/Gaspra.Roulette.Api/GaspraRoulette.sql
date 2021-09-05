SET NOCOUNT ON
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create schema: Roulette
IF NOT EXISTS (SELECT 1 FROM [sys].[schemas] WHERE name = N'Roulette')
BEGIN
    EXEC [sys].[sp_executesql] N'CREATE SCHEMA [Roulette] AUTHORIZATION [dbo]'
END
GO

-- Create table: Player
IF NOT EXISTS(SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[Player]') AND [type] IN (N'U'))
BEGIN

    CREATE TABLE [Roulette].[Player] (
        [PlayerId] [INT] IDENTITY(1,1) NOT NULL,

        [Identifier] [UNIQUEIDENTIFIER] NOT NULL,
        [Name] [NVARCHAR](255) NOT NULL,
        [TokenAllowance] [INT] NOT NULL,
        [Active] [INT] NOT NULL,

        CONSTRAINT [PK_Player_PlayerId] PRIMARY KEY CLUSTERED
        (
        	[PlayerId] ASC
        ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

END
GO

-- Create script: AddPlayer
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[AddPlayer]') AND [type] IN (N'P'))
BEGIN
    EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[AddPlayer] AS'
END
GO

ALTER PROCEDURE [Roulette].[AddPlayer]
    @Identifier UNIQUEIDENTIFIER,
    @Name NVARCHAR(255),
    @TokenAllowance INT,
    @Active INT
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    INSERT INTO
        Roulette.Player (
            Identifier,
            Name,
            TokenAllowance,
            Active
        )
    VALUES (
        @Identifier,
        @Name,
        @TokenAllowance,
        @Active
    )

END
GO

-- Create script: UpdatePlayers
IF EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[UpdatePlayers]') AND [type] IN (N'P'))
BEGIN
    DROP PROCEDURE [Roulette].[UpdatePlayers]
END
GO

IF EXISTS (SELECT 1 FROM [sys].[types] st JOIN [sys].[schemas] ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_UpdatePlayers' AND ss.name = N'Roulette')
BEGIN
    DROP TYPE [Roulette].[TT_UpdatePlayers]
END
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[types] st JOIN [sys].[schemas] ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_UpdatePlayers' AND ss.name = N'Roulette')
BEGIN
    CREATE TYPE [Roulette].[TT_UpdatePlayers] AS TABLE(
        [Identifier] [UNIQUEIDENTIFIER] NOT NULL,
        [Name] [NVARCHAR](255) NOT NULL,
        [TokenAllowance] [INT] NOT NULL,
        [Active] [INT] NOT NULL
    )
END
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[UpdatePlayers]') AND [type] IN (N'P'))
BEGIN
    EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[UpdatePlayers] AS'
END
GO

ALTER PROCEDURE [Roulette].[UpdatePlayers]
    @Players [Roulette].[TT_UpdatePlayers] READONLY
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    MERGE
        Roulette.Player AS t
    USING
        @Players AS s
    ON
        (t.Identifier = s.Identifier)
    WHEN MATCHED
        THEN UPDATE SET
            t.Active = s.Active,
            t.TokenAllowance = s.TokenAllowance,
            t.Name = s.Name
    ;

END
GO


-- Create script: GetPlayers
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[GetPlayers]') AND [type] IN (N'P'))
BEGIN
    EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[GetPlayers] AS'
END
GO

ALTER PROCEDURE [Roulette].[GetPlayers]
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
        p.Identifier,
        p.Name,
        p.TokenAllowance,
        p.Active
    FROM
        Roulette.Player p

END
GO
