----------------  
--Name: KJ
--Date: 2015-09-17
--Desc: Missing index in Azure
----------------
DECLARE @Version numeric(18,10)
SET @Version = CAST(LEFT(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)),CHARINDEX('.',CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max))) - 1) + '.' + REPLACE(RIGHT(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)), LEN(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max))) - CHARINDEX('.',CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)))),'.','') AS numeric(18,10))

IF SERVERPROPERTY('EngineEdition') = 5 AND @Version < 12
BEGIN
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_scenario]') AND name = N'IX_scenario_code')
		CREATE NONCLUSTERED INDEX [IX_scenario_code] ON [mart].[dim_scenario]
		(
			[scenario_code] ASC
		)

END