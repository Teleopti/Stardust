----------------  
--Name: JN
--Date: 2012-02-01
--Desc: Add new report selection control an use it on three reports
----------------
INSERT INTO mart.report_control
SELECT 40, 'twolistActivity', 'mart.report_control_twolist_activity_get', null


UPDATE mart.report_control_collection 
SET control_id = 40
WHERE control_collection_id IN (322, 343, 449)

insert into mart.report_control
select 41, 'cboRequest','mart.report_control_request_type_get',NULL

----------------  
--Name: Talha + David
--Date: 2012-01-20
--Desc: Adding new stage, fact table, plus report: Request per Agent
----------------  

CREATE TABLE [stage].[stg_request](
	[date_from] [smalldatetime] NOT NULL,
	[interval_from_id] [smallint] NOT NULL,
	[request_code] [uniqueidentifier] NOT NULL,	
	[person_code] [uniqueidentifier] NOT NULL,
	[request_type_code] [tinyint] NOT NULL,
	[request_status_code] [tinyint] NOT NULL,
	[date_from_local] [smalldatetime] NOT NULL,	
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[request_day_count] [int] NOT NULL,
	[request_start_date_count] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
	[is_deleted] [smallint] NOT NULL
)
ALTER TABLE [stage].[stg_request]
ADD CONSTRAINT PK_stg_request PRIMARY KEY CLUSTERED 
(
	[date_from],
	[request_code],
	[person_code],
	[request_type_code],
	[request_status_code]
)

--Create dimension
CREATE TABLE [mart].[dim_request_type](
	[request_type_id] [tinyint] NOT NULL,
	[request_type_name] [nvarchar](50) NOT NULL,
	[resource_key] [nvarchar](100) NOT NULL
)

ALTER TABLE [mart].[dim_request_type]
ADD CONSTRAINT PK_dim_request_type PRIMARY KEY CLUSTERED 
(
	[request_type_id] ASC
)

--Fill static dimension
INSERT INTO mart.dim_request_type(request_type_id,request_type_name,resource_key) VALUES (0,'Text','ResRequestTypeText')
INSERT INTO mart.dim_request_type(request_type_id,request_type_name,resource_key) VALUES (1,'Absence','ResRequestTypeAbsence')
INSERT INTO mart.dim_request_type(request_type_id,request_type_name,resource_key) VALUES (2,'Shift Trade','ResRequestTypeShiftTrade')
GO 

CREATE TABLE [mart].[dim_request_status](
	[request_status_id] [tinyint] NOT NULL,
	[request_status_name] [nvarchar](50) NOT NULL,
	[resource_key] [nvarchar](100) NOT NULL
)

ALTER TABLE [mart].[dim_request_status]
ADD CONSTRAINT PK_dim_request_status PRIMARY KEY CLUSTERED 
(
	[request_status_id] ASC
)

--Fill static dimension
INSERT INTO mart.dim_request_status(request_status_id,request_status_name,resource_key) VALUES (0,'Pending','ResRequestStatusPending')
INSERT INTO mart.dim_request_status(request_status_id,request_status_name,resource_key) VALUES (1,'Approved','ResRequestStatusApproved')
INSERT INTO mart.dim_request_status(request_status_id,request_status_name,resource_key) VALUES (2,'Denied','ResRequestStatusDenied')
GO 


--Create fact
CREATE TABLE [mart].[fact_request](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL,
	[request_type_id] [tinyint] NOT NULL,
	[request_status_id] [tinyint] NOT NULL,
	[date_id_local] [int] NOT NULL,
	[request_start_date_count] [int] NOT NULL,	
	[request_day_count] [int] NOT NULL,
	[business_unit_id] [smallint] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL
 )

ALTER TABLE mart.fact_request ADD CONSTRAINT
	  PK_fact_request PRIMARY KEY CLUSTERED 
	  (
	  date_id,
	  interval_id,
	  person_id,
	  request_type_id,
	  request_status_id
	  )

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_interval_UTC] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])

ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_interval_UTC]
GO

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_date_local] FOREIGN KEY([date_id_local])
REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_date_local]
GO

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_date_UTC] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_date_UTC]
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


	declare @reportId int
	declare @CollectionId int
	declare @url nvarchar(500)
	declare @reportName nvarchar(500)
	declare @report_name_resource_key nvarchar(50)
	declare @rpt_file_name nvarchar(100)
	declare @proc_name nvarchar(100)
	declare @help_key nvarchar(500)

	set @reportId=27 --select * from  mart.report order by 1 desc (Your new report_id needs to be uniuqe and hard coded/fixed)
	set @CollectionId=33 -- --select distinct collection_id from  mart.report_control_collection order by 1 desc (You might be able to use an existing report colletion (= collation of selection page drop downs).
	set @reportName='Requests Per Agent' --Report name
	set @url='~/Selection.aspx?ReportID='+CAST(@reportId as varchar(3))
	set @report_name_resource_key='ResReportRequestsPerAgent' --Report name. Add this to /Text/Resources.resx + Resources.sv.resx
	set @rpt_file_name='~/Reports/CCC/report_requests_per_agent.rdlc' --new RDLC file
	set @proc_name='mart.report_data_requests_per_agent' --the data SP to be called by RDLC
	set @help_key='f01_Report_RequestsPerAgent.html' --The wiki page

	insert into mart.report
	select @reportId, @CollectionId, 1,@url,'_blank',@reportName,@report_name_resource_key,1,@rpt_file_name,1000,@proc_name,@help_key,'','','',''

	-------------
	--Add selection params
	--Tricky part. Be relaxed and concentrate :-). This is where the order and dependency for the user input in the selection page is set
	-------------
	INSERT INTO [mart].[report_control_collection]
	SELECT 370,@CollectionId,'2','1','12:00','ResDateFromColon',null,'@date_from',null,null,null,null UNION ALL
	Select 371,@CollectionId,'3','2','12:00','ResDateToColon',null,'@date_to','370',null,null,null UNION ALL
	Select 372,@CollectionId,'4','29','-2','ResGroupPageColon',null,'@group_page_code',null,null,null,null UNION ALL
	Select 373,@CollectionId,'5','35','-99','ResGroupPageGroupColon',null,'@group_page_group_set','372',null,null,null UNION ALL
	Select 374,@CollectionId,'6','37','-2','ResAgentColon',null,'@group_page_agent_set','370','371','372',373 UNION ALL
	select 375,@CollectionId,'7','3','-2','ResSiteNameColon',null,'@site_id','370','371','372',null UNION ALL
	Select 376,@CollectionId,'8','34','-99','ResTeamNameColon',null,'@team_set','370','371','372',375 UNION ALL
	SELECT 377,@CollectionId,'8','38','-99','ResAgentsColon', NULL, '@agent_set', '370', '371', '375', '376' UNION ALL
	SELECT 378,@CollectionId,'9','41','-2','ResRequestTypeColon',NULL,'@request_type_id',null,null,null,null UNION ALL										
	SELECT 379,@CollectionId,'10','22','-1','ResTimeZoneColon',null,'@time_zone_id',null,null,null,null
	
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (352,'7.1.352') 
