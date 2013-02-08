

----------------  
--Name: David
--Date: 2013-02-07
--Desc: bug #22206
----------------  
--Check if previous table exists, in that case BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[permission_report]') AND type in (N'U'))
BEGIN
	CREATE TABLE [mart].[permission_report_A](
		[person_code] uniqueidentifier NOT NULL,
		[team_id] int NOT NULL,
		[my_own] bit NOT NULL,
		[business_unit_id] int NOT NULL,
		[datasource_id] smallint NOT NULL,
		[ReportId] uniqueidentifier NOT NULL,
		[datasource_update_date] smalldatetime NULL,
		[table_name] char(1) NOT NULL,
	CONSTRAINT [PK_permission_report_A] PRIMARY KEY CLUSTERED 
		(
			table_name ASC,
			[person_code] ASC,
			[team_id] ASC,
			[my_own] ASC,
			[business_unit_id] ASC,
			[ReportId] ASC
		),
	CONSTRAINT [CK_permission_report_A] CHECK
		(
		table_name = 'A'
		)
	)

	ALTER TABLE [mart].[permission_report_A] ADD  CONSTRAINT [DF_permission_report_A_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]

	CREATE TABLE [mart].[permission_report_B](
		[person_code] uniqueidentifier NOT NULL,
		[team_id] int NOT NULL,
		[my_own] bit NOT NULL,
		[business_unit_id] int NOT NULL,
		[datasource_id] smallint NOT NULL,
		[ReportId] uniqueidentifier NOT NULL,
		[datasource_update_date] smalldatetime NULL,
		[table_name] char(1) NOT NULL,
	CONSTRAINT [PK_permission_report_B] PRIMARY KEY CLUSTERED 
		(
			table_name ASC,
			[person_code] ASC,
			[team_id] ASC,
			[my_own] ASC,
			[business_unit_id] ASC,
			[ReportId] ASC
		),
	CONSTRAINT [CK_permission_report_B] CHECK
		(
		table_name = 'B'
		)
	)

	ALTER TABLE [mart].[permission_report_B] ADD  CONSTRAINT [DF_permission_report_B_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]

	CREATE TABLE [mart].[permission_report_active](
		[lock] [char](1) NOT NULL,
		[is_active] [char](1) NOT NULL
	)
	
	ALTER TABLE [mart].[permission_report_active] ADD CONSTRAINT PK_permission_report_active PRIMARY KEY CLUSTERED 
	(
		[lock] ASC
	)

	--only accept 'A' or 'B'
	ALTER TABLE [mart].[permission_report_active]  WITH CHECK ADD CONSTRAINT [CK_OnlyAorB] CHECK  (([is_active]='B' OR [is_active]='A'))
	--only accept one single row
	ALTER TABLE [mart].[permission_report_active]  WITH CHECK ADD CONSTRAINT [CK_OnlyOneRow] CHECK  (([lock]='x'))
	--default to 'x' for any new row
	ALTER TABLE [mart].[permission_report_active] ADD CONSTRAINT [DF_permission_report_active_lock] DEFAULT ('x') FOR [lock]
		
	INSERT INTO [mart].[permission_report_A]
		(person_code, team_id, my_own, business_unit_id, datasource_id, ReportId, datasource_update_date, table_name)
	SELECT [person_code]
		  ,[team_id]
		  ,[my_own]
		  ,[business_unit_id]
		  ,[datasource_id]
		  ,[ReportId]
		  ,[datasource_update_date]
		  ,'A'
	  FROM [mart].[permission_report]

	--set A as active
	INSERT INTO [mart].[permission_report_active] (is_active)
	SELECT 'A'

	DROP TABLE [mart].[permission_report]
END	
GO

--Drop obsolete SP
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_permission_data_check_test]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_permission_data_check_test]
GO
