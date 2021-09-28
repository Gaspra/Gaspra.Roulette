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
        [Secret] [NVARCHAR](255) NOT NULL,
        [TokenAllowance] [INT] NOT NULL,
        [TokenSpikeAllowance] [INT] NOT NULL,
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
    @Secret NVARCHAR(255),
    @TokenAllowance INT,
    @TokenSpikeAllowance INT,
    @Active INT
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    INSERT INTO
        Roulette.Player (
            Identifier,
            Name,
            Secret,
            TokenAllowance,
            TokenSpikeAllowance,
            Active
        )
    VALUES (
        @Identifier,
        @Name,
        @Secret,
        @TokenAllowance,
        @TokenSpikeAllowance,
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
        [Secret] [NVARCHAR](255) NOT NULL,
        [TokenAllowance] [INT] NOT NULL,
        [TokenSpikeAllowance] INT NOT NULL,
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
            t.TokenSpikeAllowance = s.TokenSpikeAllowance,
            t.Name = s.Name,
            t.Secret = s.Secret
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
        p.Secret,
        p.TokenAllowance,
        p.TokenSpikeAllowance,
        p.Active
    FROM
        Roulette.Player p

END
GO

-- Create script: TogglePlayer
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[TogglePlayer]') AND [type] IN (N'P'))
    BEGIN
        EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[TogglePlayer] AS'
    END
GO

ALTER PROCEDURE [Roulette].[TogglePlayer]
    @Identifier UNIQUEIDENTIFIER
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    DECLARE @Active INT = (
        SELECT
            p.Active
        FROM
            Roulette.Player p
        WHERE
            p.Identifier = @Identifier
    )

    DECLARE @Toggle INT;

    IF @Active = 1
        BEGIN
            SET @Toggle = 0
        END
    ELSE
        BEGIN
            SET @Toggle = 1
        END

    UPDATE
        Roulette.Player
    SET
        Active = @Toggle
    WHERE
        Identifier = @Identifier

END
GO

-- Create script: UpdatePlayerTokens
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[UpdatePlayerTokens]') AND [type] IN (N'P'))
    BEGIN
        EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[UpdatePlayerTokens] AS'
    END
GO

ALTER PROCEDURE [Roulette].[UpdatePlayerTokens]
    @Identifier UNIQUEIDENTIFIER,
    @TokenAllowance INT,
    @TokenSpikeAllowance INT
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    UPDATE
        Roulette.Player
    SET
        TokenAllowance = @TokenAllowance,
        TokenSpikeAllowance = @TokenSpikeAllowance
    WHERE
        Identifier = @Identifier

END
GO

-- Create script: DeletePlayer
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[DeletePlayer]') AND [type] IN (N'P'))
    BEGIN
        EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[DeletePlayer] AS'
    END
GO

ALTER PROCEDURE [Roulette].[DeletePlayer]
    @Identifier UNIQUEIDENTIFIER
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    DELETE FROM
        Roulette.Player
    WHERE
        Identifier = @Identifier

END
GO

-- Create table: History
IF NOT EXISTS(SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[History]') AND [type] IN (N'U'))
BEGIN

    CREATE TABLE [Roulette].[History] (
        [HistoryId] [INT] IDENTITY(1,1) NOT NULL,

        [Identifier] [UNIQUEIDENTIFIER] NOT NULL,
        [RollTimestamp] [DATETIMEOFFSET] NOT NULL

        CONSTRAINT [PK_History_HistoryId] PRIMARY KEY CLUSTERED
        (
        	[HistoryId] ASC
        ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

END
GO

-- Create script: AddHistory
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[AddHistory]') AND [type] IN (N'P'))
BEGIN
    EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[AddHistory] AS'
END
GO

ALTER PROCEDURE [Roulette].[AddHistory]
    @Identifier UNIQUEIDENTIFIER,
    @RollTimestamp DATETIMEOFFSET
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    INSERT INTO
        Roulette.History (
            Identifier,
            RollTimestamp
        )
    VALUES (
        @Identifier,
        @RollTimestamp
    )

END
GO

-- Create script: GetHistory
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[GetHistory]') AND [type] IN (N'P'))
    BEGIN
        EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[GetHistory] AS'
    END
GO

ALTER PROCEDURE [Roulette].[GetHistory]
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
        p.Name,
        h.RollTimestamp
    FROM
        Roulette.History h
        INNER JOIN Roulette.Player p ON h.Identifier = p.Identifier
    ORDER BY
        h.RollTimestamp DESC

END
GO


-- Create script: GetHistoryForPlayer
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[GetHistoryForPlayer]') AND [type] IN (N'P'))
BEGIN
    EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[GetHistoryForPlayer] AS'
END
GO

ALTER PROCEDURE [Roulette].[GetHistoryForPlayer]
    @Identifier UNIQUEIDENTIFIER
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    DECLARE @RollCount INT = (
        SELECT
            COUNT(h.Identifier)
        FROM
            Roulette.History h
        WHERE
            h.Identifier = @Identifier
    )

    DECLARE @HistoryCount INT = (
        SELECT
            COUNT(h.Identifier)
        FROM
            Roulette.History h
    )

    SELECT
        @RollCount AS RollCount,
        @HistoryCount AS HistoryCount

END
GO

-- Create table: GameRule
IF NOT EXISTS(SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[GameRule]') AND [type] IN (N'U'))
BEGIN

    CREATE TABLE [Roulette].[GameRule] (
        [GameRuleId] [INT] IDENTITY(1,1) NOT NULL,

        [RuleType] [NVARCHAR](255) NOT NULL,
        [Value] [NVARCHAR](255) NOT NULL,

        CONSTRAINT [PK_GameRule_GameRuleId] PRIMARY KEY CLUSTERED
        (
        	[GameRuleId] ASC
        ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

END
GO

-- Create script: GetRollInterval
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[GetRollInterval]') AND [type] IN (N'P'))
    BEGIN
        EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[GetRollInterval] AS'
    END
GO

ALTER PROCEDURE [Roulette].[GetRollInterval]
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
        CONVERT(INT, gr.Value) AS RollInterval
    FROM
        Roulette.GameRule gr
    WHERE
        gr.RuleType = 'RollInterval'

END
GO

-- Create script: UpdateRollInterval
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[UpdateRollInterval]') AND [type] IN (N'P'))
    BEGIN
        EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[UpdateRollInterval] AS'
    END
GO

ALTER PROCEDURE [Roulette].[UpdateRollInterval]
    @RollInterval INT
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    IF EXISTS (SELECT 1 FROM Roulette.GameRule WHERE RuleType = 'RollInterval')
    BEGIN

        UPDATE
            Roulette.GameRule
        SET
            Value = CONVERT(NVARCHAR(255), @RollInterval)
        WHERE
            RuleType = 'RollInterval'

    END
    ELSE
    BEGIN

        INSERT
            Roulette.GameRule (RuleType, Value)
        VALUES
            ('RollInterval', CONVERT(NVARCHAR(255), @RollInterval))

    END
END
GO

-- Create script: GetSpikeTokenAllocation
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[GetSpikeTokenAllocation]') AND [type] IN (N'P'))
    BEGIN
        EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[GetSpikeTokenAllocation] AS'
    END
GO

ALTER PROCEDURE [Roulette].[GetSpikeTokenAllocation]
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
        gr.Value AS SpikeTokenAllocation
    FROM
        Roulette.GameRule gr
    WHERE
        gr.RuleType = 'SpikeTokenAllocation'

END
GO

-- Create script: UpdateSpikeTokenAllocation
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[UpdateSpikeTokenAllocation]') AND [type] IN (N'P'))
    BEGIN
        EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[UpdateSpikeTokenAllocation] AS'
    END
GO

ALTER PROCEDURE [Roulette].[UpdateSpikeTokenAllocation]
    @MinWinnerTokens INT,
    @MaxWinnerTokens INT,
    @MinLoserTokens INT,
    @MaxLoserTokens INT
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    IF EXISTS (SELECT 1 FROM Roulette.GameRule WHERE RuleType = 'SpikeTokenAllocation')
        BEGIN

            UPDATE
                Roulette.GameRule
            SET
                Value = CONVERT(NVARCHAR(50), @MinWinnerTokens) + ',' +
                        CONVERT(NVARCHAR(50), @MaxWinnerTokens) + ',' +
                        CONVERT(NVARCHAR(50), @MinLoserTokens) + ',' +
                        CONVERT(NVARCHAR(50), @MaxLoserTokens)
            WHERE
                RuleType = 'SpikeTokenAllocation'

        END
    ELSE
        BEGIN

            INSERT
                Roulette.GameRule (RuleType, Value)
            VALUES
            ('SpikeTokenAllocation',
             CONVERT(NVARCHAR(50), @MinWinnerTokens) + ',' +
             CONVERT(NVARCHAR(50), @MaxWinnerTokens) + ',' +
             CONVERT(NVARCHAR(50), @MinLoserTokens) + ',' +
             CONVERT(NVARCHAR(50), @MaxLoserTokens))

        END
END
GO

-- Create script: ResetEverything
IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[Roulette].[ResetEverything]') AND [type] IN (N'P'))
    BEGIN
        EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [Roulette].[ResetEverything] AS'
    END
GO

ALTER PROCEDURE [Roulette].[ResetEverything]
AS
BEGIN

    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    TRUNCATE TABLE Roulette.History;

    TRUNCATE TABLE Roulette.Player;

    TRUNCATE TABLE Roulette.GameRule;

    EXEC Roulette.UpdateRollInterval 600;

    EXEC Roulette.UpdateSpikeTokenAllocation 2, 6, 0, 1

END
GO
