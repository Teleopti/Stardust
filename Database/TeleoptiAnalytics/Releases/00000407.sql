--Name: Team Green
--Date: 2014-10-07
--Desc: Removed the ActualAgentState history table
IF OBJECT_ID('RTA.ActualAgentState_History') IS NOT NULL
BEGIN
	DROP TABLE RTA.ActualAgentState_History
END
GO

insert mart.sys_configuration ([key], [value] )values( 'PBI30787OnlyLatestQueueAgentStatistics', 'False')
GO

-------------
--PBI #30787 - new tables to store last agg date and interval
-------------
--implemented in code instead
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossdatabaseview]') AND type in (N'U'))
DROP TABLE [mart].[sys_crossdatabaseview]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossDatabaseView_Stubs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_crossDatabaseView_Stubs]
GO
--new tables
CREATE TABLE [mart].[sys_datasource_detail_type](
	[detail_id] int NOT NULL,
	[detail_desc] varchar(50) NOT NULL,
	CONSTRAINT [PK_sys_datasource_detail_type] PRIMARY KEY CLUSTERED 
(
	[detail_id] ASC
)
)
GO

INSERT INTO [mart].[sys_datasource_detail_type]
SELECT 1,'Agent'
UNION ALL
SELECT 2,'Queue'
GO
--drop table [mart].[sys_datasource_detail]
CREATE TABLE [mart].[sys_datasource_detail](
	[datasource_id] smallint NOT NULL,
	[detail_id] int NOT NULL,
	[target_date_local] smalldatetime NOT NULL,
	[target_interval_local] smallint NOT NULL,
	[intervals_back] smallint NOT NULL
	CONSTRAINT [PK_sys_datasource_detail] PRIMARY KEY CLUSTERED 
(
	[datasource_id] ASC,
	[detail_id] ASC
)
)
GO

ALTER TABLE [mart].[sys_datasource_detail]  WITH CHECK ADD  CONSTRAINT [FK_sys_datasource_detail_sys_datasource] FOREIGN KEY([datasource_id])
REFERENCES [mart].[sys_datasource] ([datasource_id])

ALTER TABLE [mart].[sys_datasource_detail] CHECK CONSTRAINT [FK_sys_datasource_detail_sys_datasource]

ALTER TABLE [mart].[sys_datasource_detail]  WITH CHECK ADD  CONSTRAINT [FK_sys_datasource_detail_sys_datasource_detail_type] FOREIGN KEY([detail_id])
REFERENCES [mart].[sys_datasource_detail_type] ([detail_id])

ALTER TABLE [mart].[sys_datasource_detail] CHECK CONSTRAINT [FK_sys_datasource_detail_sys_datasource_detail_type]
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (407,'8.1.407') 
