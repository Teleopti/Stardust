/* 
BuildTime is: 
2009-04-16 
11:49
*/ 
/* 
Trunk initiated: 
2009-04-03 
14:20
By: TOPTINET\zoet 
On TELEOPTI558 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  

----------------  
--Name: KJ
--Date: 2009-04-06 
--Desc: New column person_period_code in Stage.stg_person 
----------------  

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_datasource_id
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_insert_date
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_update_date
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_datasource_update_date
GO
CREATE TABLE stage.Tmp_stg_person
	(
	person_code uniqueidentifier NOT NULL,
	valid_from_date smalldatetime NOT NULL,
	valid_to_date smalldatetime NOT NULL,
	person_period_code uniqueidentifier NULL,
	person_first_name nvarchar(25) NOT NULL,
	person_last_name nvarchar(25) NOT NULL,
	team_code uniqueidentifier NOT NULL,
	team_name nvarchar(50) NOT NULL,
	site_code uniqueidentifier NOT NULL,
	site_name nvarchar(50) NOT NULL,
	business_unit_code uniqueidentifier NOT NULL,
	business_unit_name nvarchar(50) NOT NULL,
	email nvarchar(200) NULL,
	note nchar(1024) NULL,
	employment_number nvarchar(50) NULL,
	employment_start_date smalldatetime NOT NULL,
	employment_end_date smalldatetime NOT NULL,
	time_zone_code nvarchar(50) NULL,
	is_agent bit NULL,
	is_user bit NULL,
	contract_code uniqueidentifier NULL,
	contract_name nvarchar(50) NULL,
	parttime_code uniqueidentifier NULL,
	parttime_percentage nvarchar(50) NULL,
	employment_type nvarchar(50) NULL,
	datasource_id smallint NOT NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL,
	datasource_update_date smalldatetime NOT NULL
	)  ON STAGE
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_datasource_id DEFAULT ((1)) FOR datasource_id
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_update_date DEFAULT (getdate()) FOR update_date
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_datasource_update_date DEFAULT (getdate()) FOR datasource_update_date
GO
IF EXISTS(SELECT * FROM stage.stg_person)
	 EXEC('INSERT INTO stage.Tmp_stg_person (person_code, valid_from_date, valid_to_date, person_first_name, person_last_name, team_code, team_name, site_code, site_name, business_unit_code, business_unit_name, email, note, employment_number, employment_start_date, employment_end_date, time_zone_code, is_agent, is_user, contract_code, contract_name, parttime_code, parttime_percentage, employment_type, datasource_id, insert_date, update_date, datasource_update_date)
		SELECT person_code, valid_from_date, valid_to_date, person_first_name, person_last_name, team_code, team_name, site_code, site_name, business_unit_code, business_unit_name, email, note, employment_number, employment_start_date, employment_end_date, time_zone_code, is_agent, is_user, contract_code, contract_name, parttime_code, parttime_percentage, employment_type, datasource_id, insert_date, update_date, datasource_update_date FROM stage.stg_person WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE stage.stg_person
GO
EXECUTE sp_rename N'stage.Tmp_stg_person', N'stg_person', 'OBJECT' 
GO
ALTER TABLE stage.stg_person ADD CONSTRAINT
	PK_stg_person PRIMARY KEY CLUSTERED 
	(
	person_code,
	valid_from_date
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON STAGE

GO
COMMIT
GO

----------------  
--Name: KJ
--Date: 2009-04-06 
--Desc: New column person_period_code in mart.dim_person 
----------------  
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT FK_dim_person_dim_skillset
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_valid_to_date
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_person_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_first_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_last_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_employment_type_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_tema_id
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_team_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_site_id
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_site_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_business_unit_id
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_business_unit_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_datasource_id
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_insert_date
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_update_date
GO
CREATE TABLE mart.Tmp_dim_person
	(
	person_id int NOT NULL IDENTITY (1, 1),
	person_code uniqueidentifier NULL,
	valid_from_date smalldatetime NOT NULL,
	valid_to_date smalldatetime NOT NULL,
	person_period_code uniqueidentifier NULL,
	person_name nvarchar(200) NOT NULL,
	first_name nvarchar(30) NOT NULL,
	last_name nvarchar(30) NOT NULL,
	employment_number nvarchar(50) NULL,
	employment_type_code int NULL,
	employment_type_name nvarchar(50) NOT NULL,
	contract_code uniqueidentifier NULL,
	contract_name nvarchar(50) NULL,
	parttime_code uniqueidentifier NULL,
	parttime_percentage nvarchar(50) NULL,
	team_id int NULL,
	team_code uniqueidentifier NULL,
	team_name nvarchar(50) NOT NULL,
	site_id int NULL,
	site_code uniqueidentifier NULL,
	site_name nvarchar(50) NOT NULL,
	business_unit_id int NULL,
	business_unit_code uniqueidentifier NULL,
	business_unit_name nvarchar(50) NOT NULL,
	skillset_id int NULL,
	email nvarchar(200) NULL,
	note nvarchar(1024) NULL,
	employment_start_date smalldatetime NULL,
	employment_end_date smalldatetime NULL,
	time_zone_id int NULL,
	is_agent bit NULL,
	is_user bit NULL,
	datasource_id smallint NOT NULL,
	insert_date smalldatetime NOT NULL,
	update_date smalldatetime NOT NULL,
	datasource_update_date smalldatetime NULL
	)  ON MART
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_valid_to_date DEFAULT (((2059)-(12))-(31)) FOR valid_to_date
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_person_name DEFAULT (N'Not Defined') FOR person_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_first_name DEFAULT (N'Not Defined') FOR first_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_last_name DEFAULT (N'Not Defined''') FOR last_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_employment_type_name DEFAULT (N'Not Defined') FOR employment_type_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_tema_id DEFAULT ((-1)) FOR team_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_team_name DEFAULT (N'Not Defined') FOR team_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_site_id DEFAULT ((-1)) FOR site_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_site_name DEFAULT (N'Not Defined') FOR site_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_business_unit_id DEFAULT ((-1)) FOR business_unit_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_business_unit_name DEFAULT (N'Not Defined') FOR business_unit_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_datasource_id DEFAULT ((-1)) FOR datasource_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_update_date DEFAULT (getdate()) FOR update_date
GO
SET IDENTITY_INSERT mart.Tmp_dim_person ON
GO
IF EXISTS(SELECT * FROM mart.dim_person)
	 EXEC('INSERT INTO mart.Tmp_dim_person (person_id, person_code, valid_from_date, valid_to_date, person_name, first_name, last_name, employment_number, employment_type_code, employment_type_name, contract_code, contract_name, parttime_code, parttime_percentage, team_id, team_code, team_name, site_id, site_code, site_name, business_unit_id, business_unit_code, business_unit_name, skillset_id, email, note, employment_start_date, employment_end_date, time_zone_id, is_agent, is_user, datasource_id, insert_date, update_date, datasource_update_date)
		SELECT person_id, person_code, valid_from_date, valid_to_date, person_name, first_name, last_name, employment_number, employment_type_code, employment_type_name, contract_code, contract_name, parttime_code, parttime_percentage, team_id, team_code, team_name, site_id, site_code, site_name, business_unit_id, business_unit_code, business_unit_name, skillset_id, email, note, employment_start_date, employment_end_date, time_zone_id, is_agent, is_user, datasource_id, insert_date, update_date, datasource_update_date FROM mart.dim_person WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT mart.Tmp_dim_person OFF
GO
ALTER TABLE mart.fact_schedule_deviation
	DROP CONSTRAINT FK_fact_schedule_deviation_dim_person
GO
ALTER TABLE mart.fact_schedule_preference
	DROP CONSTRAINT FK_fact_schedule_preference_dim_person
GO
ALTER TABLE mart.bridge_acd_login_person
	DROP CONSTRAINT FK_bridge_acd_login_person_dim_person
GO
ALTER TABLE mart.fact_contract
	DROP CONSTRAINT FK_fact_contract_time_dim_person
GO
ALTER TABLE mart.fact_schedule
	DROP CONSTRAINT FK_fact_schedule_dim_person
GO
ALTER TABLE mart.fact_schedule_day_count
	DROP CONSTRAINT FK_fact_schedule_day_count_dim_person
GO
DROP TABLE mart.dim_person
GO
EXECUTE sp_rename N'mart.Tmp_dim_person', N'dim_person', 'OBJECT' 
GO
ALTER TABLE mart.dim_person ADD CONSTRAINT
	PK_dim_person PRIMARY KEY CLUSTERED 
	(
	person_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.dim_person ADD CONSTRAINT
	FK_dim_person_dim_skillset FOREIGN KEY
	(
	skillset_id
	) REFERENCES mart.dim_skillset
	(
	skillset_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_schedule_day_count ADD CONSTRAINT
	FK_fact_schedule_day_count_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_schedule ADD CONSTRAINT
	FK_fact_schedule_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_contract ADD CONSTRAINT
	FK_fact_contract_time_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.bridge_acd_login_person ADD CONSTRAINT
	FK_bridge_acd_login_person_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_schedule_preference ADD CONSTRAINT
	FK_fact_schedule_preference_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_schedule_deviation ADD CONSTRAINT
	FK_fact_schedule_deviation_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
GO


----------------  
--Name: Anders F  
--Date: 2009-04-07  
--Desc: tcpip communication in MB  
----------------  
BEGIN TRANSACTION
GO
CREATE TABLE msg.Tmp_Address
	(
	AddressId int NOT NULL,
	[Address] nvarchar(15) NOT NULL,
	Port int NOT NULL
	)  ON msg
GO
IF EXISTS(SELECT * FROM msg.Address)
	 EXEC('INSERT INTO msg.Tmp_Address (AddressId, Address, Port)
		SELECT MessageBrokerId, MulticastAddress, Port FROM msg.Address WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE msg.Address
GO
EXECUTE sp_rename N'msg.Tmp_Address', N'Address', 'OBJECT' 
GO
ALTER TABLE msg.Address ADD CONSTRAINT
	PK_Address_1 PRIMARY KEY CLUSTERED 
	(
	AddressId
	)  ON MSG

GO

DROP TABLE [msg].[Heartbeat]
GO

CREATE TABLE [msg].[Heartbeat]
(
[HeartbeatId] [uniqueidentifier] NOT NULL,
[SubscriberId] [uniqueidentifier] NOT NULL,
[ProcessId] [int] NOT NULL,
[ChangedBy] [nvarchar] (50) NOT NULL,
[ChangedDateTime] [datetime] NOT NULL
) ON msg
GO

ALTER TABLE [msg].[Heartbeat] ADD CONSTRAINT [PK_Heartbeat] PRIMARY KEY NONCLUSTERED  ([HeartbeatId]) ON MSG
GO
CREATE NONCLUSTERED INDEX [IX_Heartbeat_ChangedBy] ON [msg].[Heartbeat] ([ChangedBy]) ON MSG
GO
CREATE NONCLUSTERED INDEX [IX_Heartbeat_ChangedDateTime] ON [msg].[Heartbeat] ([ChangedDateTime]) ON MSG
GO

ALTER TABLE [msg].[Subscriber] ADD
[IpAddress] [nvarchar] (15) NOT NULL CONSTRAINT [DF_Subscriber_IPAddress] DEFAULT (N'169.0.0.1'),
[Port] [int] NOT NULL CONSTRAINT [DF_Subscriber_Port] DEFAULT ((9080))
GO

IF NOT EXISTS (SELECT 1 FROM [msg].[Configuration] WHERE ConfigurationId = 10)
INSERT INTO [msg].[Configuration] VALUES (10,'TeleoptiBrokerService','RestartTime',60000,'System.Int32',System_User,getdate())

IF NOT EXISTS (SELECT 1 FROM [msg].[Configuration] WHERE ConfigurationId = 11)
INSERT INTO [msg].[Configuration] VALUES (11,'TeleoptiBrokerService','ClientThrottle',10,'System.Int32',System_User,getdate())

IF NOT EXISTS (SELECT 1 FROM [msg].[Configuration] WHERE ConfigurationId = 12)
INSERT INTO [msg].[Configuration] VALUES (12,'TeleoptiBrokerService','ServerThrottle',10,'System.Int32',System_User,getdate())

IF NOT EXISTS (SELECT 1 FROM [msg].[Configuration] WHERE ConfigurationId = 13)
INSERT INTO [msg].[Configuration] VALUES (13,'TeleoptiBrokerService','PublisherThreads',5,'System.Int32',System_User,getdate())

IF NOT EXISTS (SELECT 1 FROM [msg].[Configuration] WHERE ConfigurationId = 14)
INSERT INTO [msg].[Configuration] VALUES (14,'TeleoptiBrokerService','MessagingProtocol','TCPIP','System.String',System_User,getdate())

IF NOT EXISTS (SELECT 1 FROM [msg].[Configuration] WHERE ConfigurationId = 15)
INSERT INTO [msg].[Configuration] VALUES (15,'TeleoptiBrokerService','TimeToLive',1,'System.Int32',System_User,getdate())

CREATE TABLE [msg].[Pending]
(
[SubscriberId] [uniqueidentifier] NOT NULL
) on msg
GO

--EXEC sp_refreshview N'[msg].[vCurrentUsers]'
--GO

COMMIT TRAN
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_csharp]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_csharp]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_delete]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_insert]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_select]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_update]
GO

----------------  
--Name: Peter A/Anders F  
--Date: 2009-04-07  
--Desc: Added parent stuff in messages  
----------------  
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Filter]') AND type in (N'U'))
DROP TABLE [msg].[Filter]
GO
CREATE TABLE [msg].[Filter](
	[FilterId] [uniqueidentifier] NOT NULL,
	[SubscriberId] [uniqueidentifier] NOT NULL,
	[ParentObjectId] [uniqueidentifier] NOT NULL,
	[ParentObjectType] [nvarchar](255) NOT NULL,
	[DomainObjectId] [uniqueidentifier] NOT NULL,
	[DomainObjectType] [nvarchar](255) NOT NULL,
	[EventStartDate] [datetime] NOT NULL,
	[EventEndDate] [datetime] NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Filter] PRIMARY KEY NONCLUSTERED 
(
	[FilterId] ASC
) ON [MSG]
) ON [MSG]
GO 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Event]') AND type in (N'U'))
DROP TABLE [msg].[Event]
GO
CREATE TABLE [msg].[Event](
	[EventId] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[ModuleId] [uniqueidentifier] NOT NULL,
	[PackageSize] [int] NOT NULL,
	[IsHeartbeat] [bit] NOT NULL,
	[ParentObjectId] [uniqueidentifier] NOT NULL,
	[ParentObjectType] [nvarchar](255) NOT NULL,
	[DomainObjectId] [uniqueidentifier] NOT NULL,
	[DomainObjectType] [nvarchar](255) NOT NULL,
	[DomainUpdateType] [int] NOT NULL,
	[DomainObject] [varbinary](1024) NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Event] PRIMARY KEY NONCLUSTERED 
(
	[EventId] ASC
) ON [MSG]
) ON [MSG]
 
GO 
 
  
GO  
 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (93,'7.0.93') 
