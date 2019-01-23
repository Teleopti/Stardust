--I wonder if this one is OK to use?
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwRandom]'))
DROP VIEW [dbo].[vwRandom]
GO

CREATE VIEW dbo.vwRandom
AS
SELECT RAND() as RandomValue;
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Character_Scramble]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[Character_Scramble]
GO

CREATE FUNCTION Character_Scramble
(
    @OrigVal varchar(max)
)
RETURNS varchar(max)
WITH ENCRYPTION
AS
BEGIN

-- Variables used
DECLARE @NewVal varchar(max);
DECLARE @OrigLen int;
DECLARE @CurrLen int;
DECLARE @LoopCt int;
DECLARE @Rand int;
 
-- Set variable default values
SET @NewVal = '';
SET @OrigLen = DATALENGTH(@OrigVal);
SET @CurrLen = @OrigLen;
SET @LoopCt = 1;
 
-- Loop through the characters passed
WHILE @LoopCt <= @OrigLen
    BEGIN
        -- Current length of possible characters
        SET @CurrLen = DATALENGTH(@OrigVal);
   
        -- Random position of character to use
        SELECT
            @Rand = Convert(int,(((1) - @CurrLen) *  
                               RandomValue + @CurrLen))
        FROM
            dbo.vwRandom;
           
        -- Assembles the value to be returned
        SET @NewVal = @NewVal +
                             SUBSTRING(@OrigVal,@Rand,1);
       
        -- Removes the character from available options
        SET @OrigVal =
                 Replace(@OrigVal,SUBSTRING(@OrigVal,@Rand,1),'');
 
        -- Advance the loop
        SET @LoopCt = @LoopCt + 1;
    END
    -- Returns new value
    Return LOWER(@NewVal);
END
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Numeric_Variance]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[Numeric_Variance]
GO

-- Create user defined function
CREATE FUNCTION dbo.Numeric_Variance
(
    @OrigVal float,
    @VarPercent numeric(5,2)
)
RETURNS float
WITH ENCRYPTION
AS
BEGIN

    -- Variable used
    DECLARE @Rand float;
   
    -- Random position of character to use
    SELECT
        @Rand = Convert(float,(((((0-@VarPercent)+1) - @VarPercent) * RandomValue + @VarPercent))/100)
    FROM
        dbo.vwRandom;
    RETURN @OrigVal - CONVERT(INT,(@OrigVal*@Rand));
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_scramble_All_Data]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_scramble_All_Data]
GO

CREATE PROCEDURE dbo.sp_scramble_All_Data
AS
BEGIN

if (DB_NAME()='TeleoptiCCC7')
BEGIN
	DECLARE @DBID INT;
	SET @DBID = DB_ID();

	DECLARE @DBNAME NVARCHAR(128);
	SET @DBNAME = DB_NAME();
	RAISERROR
		(N'The current database name is: %s. This stored procedure must NOT(!) be execute in production database. Please make a copy and try again.',
		16, -- Severity.
		1, -- State.
		@DBID, -- First substitution argument.
		@DBNAME); -- Second substitution argument.
END
	--===================
	--Organization
	--===================
	--update all varchars strings that could identify/expose organization
	update a
	set
		Name		= dbo.Character_Scramble(a.Name),
		ShortName	= dbo.Character_Scramble(a.ShortName)
	from Team a

	update a
	set
		Name		= dbo.Character_Scramble(a.Name),
		ShortName	= dbo.Character_Scramble(a.ShortName)
	from Site a

	update a
	set
		Name		= dbo.Character_Scramble(a.Name),
		ShortName	= dbo.Character_Scramble(a.ShortName)
	from BusinessUnit a

	--===================
	--Forecast and Skill
	--===================
	--update all varchars strings that could identify/expose forecast names and skills
	update a set
		Name		= dbo.Character_Scramble(a.Name)
	from Skill a

	update a set
		Name		= dbo.Character_Scramble(a.Name),
		ShortName	= dbo.Character_Scramble(a.ShortName)
	from SkillType a

	update a set
		TemplateName		= dbo.Character_Scramble(a.TemplateName)
	from MultisiteDay a

	update a set
		Name		= dbo.Character_Scramble(a.Name)
	from MultisiteDayTemplate a

	update a set
		Name		= dbo.Character_Scramble(a.Name)
	from Workload a

	--also make sure we destroy the forecast data:
	update a set
		Tasks					= 1,
		AverageTaskTime			= 1,
		AverageAfterTaskTime	= 1,
		CampaignTasks			= 1,
		CampaignTaskTime		= 1,
		CampaignAfterTaskTime	= 1
	from TemplateTaskPeriod a

	update a set
		Value					= '0.2',--default value
		Seconds					= 1,
		MaxOccupancy			= '0.9',--default value
		MinOccupancy			= '0.2',--default value
		PersonBoundary_Maximum	= 0,
		PersonBoundary_Minimum	= 0,
		Shrinkage				= '0.1',--default value
		Efficiency				= '0.9'--default value
	from TemplateSkillDataPeriod a

	--===================
	--CTI data
	--===================
	--update all varchars that could identify/expose Queues and ACD Id
	select * from ExternalLogOn a
	update a set
--		a.AcdLogOnMartId	= dbo.Character_Scramble(a.AcdLogOnMartId,20),
--		a.AcdLogOnAggId		= dbo.Character_Scramble(a.AcdLogOnAggId,20),
		a.AcdLogOnOriginalId= dbo.Character_Scramble(a.AcdLogOnOriginalId),
		a.AcdLogOnName		= dbo.Character_Scramble(a.AcdLogOnName)
	from ExternalLogOn a		
	select * from ExternalLogOn a

	select * from QueueSource
	update a set
		a.LogObjectName		= dbo.Character_Scramble(a.LogObjectName),
		a.Name				= dbo.Character_Scramble(a.Name),
		a.Description		= dbo.Character_Scramble(a.Description),
		a.QueueOriginalId	= dbo.Character_Scramble(a.QueueOriginalId)
	from QueueSource a
	select * from QueueSource

	--===================
	--personal info
	--===================
	--update all varchars that could identify/expose users
	update a set
		a.Email				= dbo.Character_Scramble(a.Email),
		a.Note				= dbo.Character_Scramble(a.Note),
		a.EmploymentNumber	= dbo.Character_Scramble(a.EmploymentNumber),
		a.FirstName			= dbo.Character_Scramble(a.FirstName),
		a.LastName			= dbo.Character_Scramble(a.LastName)
	from Person a

END

