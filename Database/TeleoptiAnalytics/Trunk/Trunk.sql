----------------  
--Name: DavidJ
--Date: 2012-02-24
--Desc: Re-factor Request per Agent
----------------
--Implemented, now re-factored. Drop if exists
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_request]') AND type in (N'U'))
DROP TABLE [stage].[stg_request]

CREATE TABLE [stage].[stg_request](
	[request_code] [uniqueidentifier] NOT NULL,	
	[person_code] [uniqueidentifier] NOT NULL,
	[application_datetime] [smalldatetime] NOT NULL,--NEW		
	[request_date] [smalldatetime] NOT NULL,--renamed from: date_from_local
	[request_startdate] [smalldatetime] NOT NULL,--NEW		
	[request_enddate] [smalldatetime] NOT NULL,	--NEW	
	[request_type_code] [tinyint] NOT NULL,
	[request_status_code] [tinyint] NOT NULL,
	[request_start_date_count] [int] NOT NULL,
	[request_day_count] [int] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
	[is_deleted] [smallint] NOT NULL
)
ALTER TABLE [stage].[stg_request]
ADD CONSTRAINT PK_stg_request PRIMARY KEY CLUSTERED 
(
	[request_code],
	[person_code],
	[request_date],
	[request_type_code],
	[request_status_code]
)

--Implemented, now re-factored. Drop if exists
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_request]') AND type in (N'U'))
DROP TABLE [mart].[fact_request]

CREATE TABLE [mart].[fact_request](	
	[request_code] [uniqueidentifier] NOT NULL,
	[person_id] [int] NOT NULL,
	[request_start_date_id] [int] NOT NULL, --NEW
	[application_datetime] [smalldatetime] NOT NULL,--NEW
	[request_startdate] [smalldatetime] NOT NULL,--NEW		
	[request_enddate] [smalldatetime] NOT NULL,	--NEW	
	[request_type_id] [tinyint] NOT NULL,
	[request_status_id] [tinyint] NOT NULL,
	[request_day_count] [int] NOT NULL,	
	[request_start_date_count] [int] NOT NULL,
	[business_unit_id] [smallint] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL
 )

ALTER TABLE mart.fact_request ADD CONSTRAINT
	  PK_fact_request PRIMARY KEY CLUSTERED 
	  (
		request_code
	  )

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_date_startdate] FOREIGN KEY([request_start_date_id])
REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_date_startdate]
GO

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_person]
GO

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_request_type] FOREIGN KEY([request_type_id])
REFERENCES [mart].[dim_request_type] ([request_type_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_request_type]
GO

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_request_status] FOREIGN KEY([request_status_id])
REFERENCES [mart].[dim_request_status] ([request_status_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_request_status]
GO

ALTER TABLE [mart].[fact_request] ADD  CONSTRAINT [DF_fact_request_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
ALTER TABLE [mart].[fact_request] ADD  CONSTRAINT [DF_fact_request_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [mart].[fact_request] ADD  CONSTRAINT [DF_fact_request_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

--Implemented, now re-factored. Drop if exists
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_requested_days]') AND type in (N'U'))
DROP TABLE [mart].[fact_requested_days]

CREATE TABLE [mart].[fact_requested_days](
	[request_code] [uniqueidentifier] NOT NULL,
	[person_id] [int] NOT NULL,
	[request_date_id] [int] NOT NULL,
	[request_type_id] [tinyint] NOT NULL,
	[request_status_id] [tinyint] NOT NULL,
	[request_day_count] [int] NOT NULL,
	[business_unit_id] [smallint] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL
 )

ALTER TABLE [mart].[fact_requested_days] ADD  CONSTRAINT [DF_fact_requested_days_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
ALTER TABLE [mart].[fact_requested_days] ADD  CONSTRAINT [DF_fact_requested_days_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [mart].[fact_requested_days] ADD  CONSTRAINT [DF_fact_requested_days_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

 ALTER TABLE mart.fact_requested_days ADD CONSTRAINT
	PK_fact_requested_days PRIMARY KEY CLUSTERED 
	(
	request_code,
	request_date_id
	)

ALTER TABLE [mart].[fact_requested_days]  WITH CHECK ADD  CONSTRAINT [FK_fact_requested_days_dim_date_local] FOREIGN KEY([request_date_id])
REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_requested_days] CHECK CONSTRAINT [FK_fact_requested_days_dim_date_local]
GO

ALTER TABLE [mart].[fact_requested_days]  WITH CHECK ADD  CONSTRAINT [FK_fact_requested_days_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
ALTER TABLE [mart].[fact_requested_days] CHECK CONSTRAINT [FK_fact_requested_days_dim_person]
GO

ALTER TABLE [mart].[fact_requested_days]  WITH CHECK ADD  CONSTRAINT [FK_fact_requested_days_dim_request_type] FOREIGN KEY([request_type_id])
REFERENCES [mart].[dim_request_type] ([request_type_id])
ALTER TABLE [mart].[fact_requested_days] CHECK CONSTRAINT [FK_fact_requested_days_dim_request_type]
GO

ALTER TABLE [mart].[fact_requested_days]  WITH CHECK ADD  CONSTRAINT [FK_fact_requested_days_dim_request_status] FOREIGN KEY([request_status_id])
REFERENCES [mart].[dim_request_status] ([request_status_id])
ALTER TABLE [mart].[fact_requested_days] CHECK CONSTRAINT [FK_fact_requested_days_dim_request_status]
GO

--Add index to support etl-load
CREATE NONCLUSTERED INDEX IX_dim_person_person_code_from_to
ON [mart].[dim_person] ([person_code],[valid_from_date],[valid_to_date])
INCLUDE ([person_id])

---------------------
--Blue/Ola custom report
---------------------
--rename existing tables and PKs
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_report_visible]') AND type = 'D')
BEGIN
	ALTER TABLE [mart].[report] DROP CONSTRAINT [DF_report_visible]
END
EXEC dbo.sp_rename @objname = N'[mart].[report]', @newname = N'report_old', @objtype = N'OBJECT'
EXEC sp_rename N'[mart].[report_old].[PK_report]', N'PK_report_old', N'INDEX'
GO

EXEC dbo.sp_rename @objname = N'[mart].[report_control]', @newname = N'report_control_old', @objtype = N'OBJECT'
EXEC sp_rename N'[mart].[report_control_old].[PK_report_control]', N'PK_report_control_old', N'INDEX'
GO
EXEC dbo.sp_rename @objname = N'[mart].[report_control_collection]', @newname = N'report_control_collection_old', @objtype = N'OBJECT'
EXEC sp_rename N'[mart].[report_control_collection_old].[PK_report_control_collection]', N'PK_report_control_collection_old', N'INDEX'
GO
EXEC dbo.sp_rename @objname = N'[mart].[report_user_setting]', @newname = N'report_user_setting_old', @objtype = N'OBJECT'
EXEC sp_rename N'[mart].[report_user_setting_old].[PK_report_user_setting]', N'PK_report_user_setting_old', N'INDEX'
GO


CREATE TABLE [mart].[report_user_setting](
	[ReportId] [uniqueidentifier] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[report_id] [int] NOT NULL,
	[param_name] [varchar](50) NOT NULL,
	[saved_name_id] [int] NOT NULL,
	[control_setting] [varchar](max) NULL
 CONSTRAINT [PK_report_user_setting] PRIMARY KEY CLUSTERED 
(
	person_code,
	param_name,
	saved_name_id,
	ReportId
)
) 

CREATE TABLE [mart].[report](
	[Id] [uniqueidentifier] NOT NULL,
	[report_id] [int] NOT NULL,
	[control_collection_id] [int] NOT NULL,
	[url] [nvarchar](500) NULL,
	[target] [nvarchar](50) NULL,
	[report_name] [nvarchar](500) NULL,
	[report_name_resource_key] [nvarchar](50) NOT NULL,
	[visible] [bit] NOT NULL,
	[rpt_file_name] [varchar](100) NOT NULL,
	[proc_name] [varchar](100) NOT NULL,
	[help_key] [nvarchar](500) NULL,
	[sub1_name] [varchar](50) NOT NULL,
	[sub1_proc_name] [varchar](50) NOT NULL,
	[sub2_name] [varchar](50) NOT NULL,
	[sub2_proc_name] [varchar](50) NOT NULL,
	[ControlCollectionId] [uniqueidentifier] NOT NULL
 CONSTRAINT [PK_report] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

CREATE TABLE [mart].[report_control](
	[Id] [uniqueidentifier] NOT NULL,
	[control_id] [int] NOT NULL,
	[control_name] [varchar](50) NULL,
	[fill_proc_name] [varchar](200) NOT NULL	
 CONSTRAINT [PK_report_control] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) 

GO

CREATE TABLE [mart].[report_control_collection](
	[Id] [uniqueidentifier] NOT NULL,
	[ControlId] [uniqueidentifier] NOT NULL,
	[CollectionId] [uniqueidentifier] NOT NULL,
	[control_collection_id] [int] NOT NULL,
	[collection_id] [int] NOT NULL,
	[print_order] [int] NOT NULL,
	[control_id] [int] NOT NULL,
	[default_value] [nvarchar](4000) NOT NULL,
	[control_name_resource_key] [nvarchar](50) NOT NULL,
	[fill_proc_param] [varchar](100) NULL,
	[param_name] [varchar](50) NULL,
	[depend_of1] [int] NULL,
	[depend_of2] [int] NULL,
	[depend_of3] [int] NULL,
	[depend_of4] [int] NULL,
	[DependOf1] [uniqueidentifier] NULL,
	[DependOf2] [uniqueidentifier] NULL,
	[DependOf3] [uniqueidentifier] NULL,
	[DependOf4] [uniqueidentifier] NULL
 CONSTRAINT [PK_report_control_collection] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)


-----------
--mart.report
-----------
INSERT INTO [mart].[report]
SELECT
[Id] = NEWID(),
[report_id] =[report_id],
[control_collection_id]=[control_collection_id],
[url]=[url],
[target]=[target],
[report_name]=[report_name],
[report_name_resource_key]=[report_name_resource_key],
[visible]=[visible],
[rpt_file_name]=[rpt_file_name],
[proc_name]=[proc_name],
[help_key]=[help_key],
[sub1_name]=[sub1_name],
[sub1_proc_name]=[sub1_proc_name],
[sub2_name]=[sub2_name],
[sub2_proc_name]=[sub2_proc_name],
[ControlCollectionId]=newid()
FROM [mart].[report_old]

GO
-- Set fixed Guids for each report
UPDATE mart.report SET Id = '0E3F340F-C05D-4A98-AD23-A019607745C9'
WHERE report_id = 1
UPDATE mart.report SET Id = '5C133E8F-DF3E-48FC-BDEF-C6586B009481'
WHERE report_id = 2
UPDATE mart.report SET Id = '7F918C26-4044-4F6B-B0AE-7D27625D052E'
WHERE report_id = 3
UPDATE mart.report SET Id = 'C5B88862-F7BE-431B-A63F-3DD5FF8ACE54'
WHERE report_id = 4
UPDATE mart.report SET Id = '61548D4F-7D2C-4865-AB76-8A4D01800F1C'
WHERE report_id = 6
UPDATE mart.report SET Id = '720A5D88-D2B5-49E1-83EE-8D05239094BF'
WHERE report_id = 7
UPDATE mart.report SET Id = 'C232D751-AEC5-4FD7-A274-7C56B99E8DEC'
WHERE report_id = 8
UPDATE mart.report SET Id = 'AE758403-C16B-40B0-B6B2-E8F6043B6E04'
WHERE report_id = 9
UPDATE mart.report SET Id = '8D8544E4-6B24-4C1C-8083-CBE7522DD0E0'
WHERE report_id = 10
UPDATE mart.report SET Id = '71BDB56D-C12F-489B-8275-04873A668D90'
WHERE report_id = 11
UPDATE mart.report SET Id = '0065AA84-FD47-4022-ABE3-DD1B54FD096C'
WHERE report_id = 12
UPDATE mart.report SET Id = 'D1ADE4AC-284C-4925-AEDD-A193676DBD2F'
WHERE report_id = 13
UPDATE mart.report SET Id = 'F7937D02-FA54-4679-AF70-D9798E1690D5'
WHERE report_id = 14
UPDATE mart.report SET Id = '4F5DDE81-C264-4756-B1F1-F65BFE54B16B'
WHERE report_id = 15
UPDATE mart.report SET Id = '80D31D84-68DB-45A7-977F-75C3250BB37C'
WHERE report_id = 16
UPDATE mart.report SET Id = '132E3AF2-3557-4EA7-813E-05CD4869D5DB'
WHERE report_id = 17
UPDATE mart.report SET Id = '63243F7F-016E-41D1-9432-0787D26F9ED5'
WHERE report_id = 18
UPDATE mart.report SET Id = '009BCDD2-3561-4B59-A719-142CD9216727'
WHERE report_id = 19
UPDATE mart.report SET Id = '35649814-4DE8-4CB3-A51C-DDBA2A073E09'
WHERE report_id = 20
UPDATE mart.report SET Id = 'BAA446C2-C060-4F39-83EA-B836B1669331'
WHERE report_id = 21
UPDATE mart.report SET Id = 'D45A8874-57E1-4EB9-826D-E216A4CBC45B'
WHERE report_id = 22
UPDATE mart.report SET Id = 'EB977F5B-86C6-4D98-BEDF-B79DC562987B'
WHERE report_id = 23
UPDATE mart.report SET Id = '479809D8-4DAE-4852-BF67-C98C3744918D'
WHERE report_id = 24
UPDATE mart.report SET Id = 'E15400E7-892A-4EDE-9377-AE693AA56829'
WHERE report_id = 25
UPDATE mart.report SET Id = '2F222F0A-4571-4462-8FBE-0C747035994A'
WHERE report_id = 26
UPDATE mart.report SET Id = '8DE1AB0F-32C2-4619-A2B2-97385BE4C49C'
WHERE report_id = 27

GO
---------
--report_control
---------
INSERT INTO [mart].[report_control]
SELECT
id = NEWID(),
control_id=[control_id],
control_name=[control_name],
fill_proc_name=[fill_proc_name]
  FROM [mart].[report_control_old]

-- we need to know these
UPDATE mart.report_control
SET id = 'A9718D69-77A9-4D1D-9D44-DBA7EA7E92F5'
WHERE control_id = 29

UPDATE mart.report_control
SET id = '8306A37C-2134-4717-BAF6-071112AB27B6'
WHERE control_id = 30

UPDATE mart.report_control
SET id = '80770D4D-11EF-42CB-9C91-9E6A27AF35E4'
WHERE control_id = 31

UPDATE mart.report_control
SET id = 'D7E6E133-0F28-46D1-B00E-873B49C7ACDF'
WHERE control_id = 32

UPDATE mart.report_control
SET id = 'F257C91F-CD6A-4EE0-918A-3C39BD8AAF04'
WHERE control_id = 35

UPDATE mart.report_control
SET id = '5A9C7B5C-C0C6-4C31-817F-FDAA0D093B85'
WHERE control_id = 37

UPDATE mart.report_control
SET id = 'B12E74F7-48EB-4FAF-8231-B5C422F80C9A'
WHERE control_id = 39

UPDATE mart.report_control
SET id = 'AF3DCA13-71A9-4598-96A7-7EFE703F3C9F'
WHERE control_id = 3

UPDATE mart.report_control
SET id = 'E12C7EB1-23B6-4730-8019-5013C9758663'
WHERE control_id = 4

UPDATE mart.report_control
SET id = '989F6F70-29F5-43FB-9291-AA02B6503C08'
WHERE control_id = 5

UPDATE mart.report_control
SET id = 'A7EF3BC8-E7B0-4C8F-B333-FB96068A21E9'
WHERE control_id = 18

UPDATE mart.report_control
SET id = '3D4F57F4-EC28-408B-BB96-E90DEABD16AD'
WHERE control_id = 34

UPDATE mart.report_control
SET id = '6BDA3854-7915-4689-910E-9CEEE52D014B'
WHERE control_id = 36

UPDATE mart.report_control
SET id = 'EFE140D0-904A-4326-BEC2-D45945F7EC6E'
WHERE control_id = 38


GO

-----------
--report_control_collection
-----------
INSERT INTO [mart].[report_control_collection]
SELECT
[Id] = NewId(),
[ControlId] = newid(),
[CollectionId] = newid(),
[control_collection_id]= [control_collection_id],
[collection_id]= [collection_id],
[print_order]=[print_order],
[control_id]=[control_id],
[default_value]=[default_value],
[control_name_resource_key]=[control_name_resource_key],
[fill_proc_param]=[fill_proc_param],
[param_name]=[param_name],
[depend_of1]=[depend_of1],
[depend_of2]=[depend_of2],
[depend_of3]=[depend_of3],
[depend_of4]=[depend_of4],
[DependOf1] = NULL,
[DependOf2] = NULL,
[DependOf3] = NULL,
[DependOf4] = NULL
FROM [mart].[report_control_collection_old]

CREATE TABLE #collections(id int not null, collId uniqueidentifier not null)
INSERT INTO #collections SELECT DISTINCT collection_id, NEWID() FROM mart.report_control_collection

GO
UPDATE mart.report_control_collection SET CollectionId = collID
FROM mart.report_control_collection c INNER JOIN #collections
ON c.collection_id = #collections.id
GO 

UPDATE mart.report_control_collection SET ControlId = c.Id
FROM mart.report_control_collection cc INNER JOIN mart.report_control c
ON cc.control_id = c.control_id
GO 

CREATE TABLE #ids (control int ,Id uniqueidentifier)
INSERT INTO  #ids 
SELECT control_collection_id as control, Id
FROM mart.report_control_collection
 
UPDATE mart.report_control_collection SET DependOf1 = #ids.Id
FROM mart.report_control_collection  
INNER JOIN #ids
ON #ids.control = depend_of1
	
UPDATE mart.report_control_collection SET DependOf2 = #ids.Id
FROM mart.report_control_collection  
INNER JOIN #ids
ON #ids.control = depend_of2
	
UPDATE mart.report_control_collection SET DependOf3 = #ids.Id
FROM mart.report_control_collection  
INNER JOIN #ids
ON #ids.control = depend_of3
	
UPDATE mart.report_control_collection SET DependOf4 = #ids.Id
FROM mart.report_control_collection  
INNER JOIN #ids
ON #ids.control = depend_of4
GO

ALTER TABLE [mart].[report_control_collection]  WITH CHECK ADD  CONSTRAINT [FK_report_control_collection_report_control] FOREIGN KEY([ControlId])
REFERENCES [mart].[report_control] ([Id])
GO

ALTER TABLE [mart].[report_control_collection] CHECK CONSTRAINT [FK_report_control_collection_report_control]
GO

GO

UPDATE mart.report
SET url = '~/Selection.aspx?ReportId=' + CAST(Id As nvarchar(100))
WHERE SUBSTRING(url,0, 26) = '~/Selection.aspx?ReportID'

UPDATE mart.report SET ControlCollectionId = CollectionId
FROM mart.report INNER JOIN mart.report_control_collection c
ON mart.report.control_collection_id = c.collection_id
GO


INSERT INTO mart.report_user_setting
SELECT
[ReportId] = newid(),
[person_code],
[report_id],
[param_name],
[saved_name_id],
[control_setting]
FROM mart.report_user_setting_old

UPDATE mart.report_user_setting SET ReportId = Id
FROM mart.report r INNER JOIN mart.report_user_setting s
ON r.report_id = s.report_id
GO

ALTER TABLE mart.report_user_setting
DROP COLUMN report_id

ALTER TABLE mart.permission_report
ADD ReportId uniqueidentifier null
GO
UPDATE mart.permission_report SET ReportId = Id
FROM mart.report r INNER JOIN mart.permission_report s
ON r.report_id = s.report_id
GO
ALTER TABLE mart.permission_report
ALTER COLUMN ReportId uniqueidentifier not null
GO
ALTER TABLE mart.permission_report
	DROP CONSTRAINT PK_permission_report
GO
ALTER TABLE mart.permission_report ADD CONSTRAINT
	PK_permission_report PRIMARY KEY CLUSTERED 
	(
	person_code,
	team_id,
	my_own,
	business_unit_id,
	ReportId
	)

GO
ALTER TABLE mart.permission_report
	DROP CONSTRAINT FK_permission_report_data_report
GO

--safe below
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_permission_datasource_id]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_permission_report] DROP CONSTRAINT [DF_stg_permission_datasource_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_permission_insert_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_permission_report] DROP CONSTRAINT [DF_stg_permission_insert_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_permission_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_permission_report] DROP CONSTRAINT [DF_stg_permission_update_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_permission_report_datasource_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_permission_report] DROP CONSTRAINT [DF_stg_permission_report_datasource_update_date]
END

GO

/****** Object:  Table [stage].[stg_permission_report]    Script Date: 02/17/2012 11:12:20 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_permission_report]') AND type in (N'U'))
DROP TABLE [stage].[stg_permission_report]
GO

CREATE TABLE [stage].[stg_permission_report](
	[person_code] [uniqueidentifier] NULL,
	[ReportId] [uniqueidentifier] NULL,
	[team_id] [uniqueidentifier] NULL,
	[my_own] [bit] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL
	
)

GO

ALTER TABLE [stage].[stg_permission_report] ADD  CONSTRAINT [DF_stg_permission_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO

ALTER TABLE [stage].[stg_permission_report] ADD  CONSTRAINT [DF_stg_permission_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [stage].[stg_permission_report] ADD  CONSTRAINT [DF_stg_permission_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

ALTER TABLE [stage].[stg_permission_report] ADD  CONSTRAINT [DF_stg_permission_report_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO

CREATE CLUSTERED INDEX [CIX_stg_permisssion_report] ON [stage].[stg_permission_report] 
(
	[person_code] ASC
)
GO


-- remove the old and unused stuff
ALTER TABLE mart.permission_report DROP COLUMN report_id
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_report_report_group]') AND parent_object_id = OBJECT_ID(N'[mart].[report]'))
ALTER TABLE [mart].[report] DROP CONSTRAINT [FK_report_report_group]
GO

--drop temp tables
DROP TABLE mart.report_old
GO
DROP TABLE mart.report_control_collection_old
GO
DROP TABLE mart.report_control_old
GO
DROP TABLE mart.report_user_setting_old
GO



--ALTER TABLE mart.report DROP COLUMN report_group_id

DROP TABLE mart.report_group
GO

CREATE TABLE [mart].[custom_report_control](
	[control_name] [varchar](50) NULL,
	[fill_proc_name] [varchar](200) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_custom_report_control] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

GO

CREATE TABLE [mart].[custom_report_control_collection](
	[Id] [uniqueidentifier] NOT NULL,
	[CollectionId] [uniqueidentifier] NOT NULL,
	[ControlId] [uniqueidentifier] NOT NULL,
	[print_order] [int] NOT NULL,
	[default_value] [nvarchar](4000) NOT NULL,
	[control_name_resource_key] [nvarchar](50) NOT NULL,
	[fill_proc_param] [varchar](100) NULL,
	[param_name] [varchar](50) NULL,
	[DependOf1] [uniqueidentifier] NULL,
	[DependOf2] [uniqueidentifier] NULL,
	[DependOf3] [uniqueidentifier] NULL,
	[DependOf4] [uniqueidentifier] NULL,
 CONSTRAINT [PK_custom_report_control_collection] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

GO

ALTER TABLE [mart].[custom_report_control_collection]  WITH CHECK ADD  CONSTRAINT [FK_custom_report_control_collection_report_control] FOREIGN KEY([ControlId])
REFERENCES [mart].[custom_report_control] ([Id])
GO

ALTER TABLE [mart].[custom_report_control_collection] CHECK CONSTRAINT [FK_custom_report_control_collection_report_control]
GO

CREATE TABLE [mart].[custom_report](
	[url] [nvarchar](500) NULL,
	[target] [nvarchar](50) NULL,
	[report_name] [nvarchar](500) NULL,
	[report_name_resource_key] [nvarchar](50) NOT NULL,
	[visible] [bit] NOT NULL,
	[rpt_file_name] [varchar](100) NOT NULL,
	[proc_name] [varchar](100) NOT NULL,
	[help_key] [nvarchar](500) NULL,
	[sub1_name] [varchar](50) NOT NULL,
	[sub1_proc_name] [varchar](50) NOT NULL,
	[sub2_name] [varchar](50) NOT NULL,
	[sub2_proc_name] [varchar](50) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[ControlCollectionId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_custom_report] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

GO

ALTER TABLE [mart].[custom_report] ADD  CONSTRAINT [DF_custom_report_visible]  DEFAULT ((1)) FOR [visible]
GO
