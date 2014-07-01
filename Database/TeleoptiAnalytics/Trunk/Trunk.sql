

----------------  
--Name: David Jonsson
--Date: 2014-07-01
--Desc: Bug #27933 - Give the end user a posibility to cherrypick tables for update_stat
----------------
SET NOCOUNT OFF
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_updatestat_tables]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[sys_updatestat_tables](
	table_schema sysname NOT NULL,
	table_name sysname NOT NULL,
	options NVARCHAR(200) NULL
	CONSTRAINT [PK_sys_updatestat_tables] PRIMARY KEY CLUSTERED 
	(
		table_schema ASC,
		table_name ASC
	)
)
END

GO
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'sqlserver_updatestat' AND jobstep_id=87)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(87,N'sqlserver_updatestat')
GO

