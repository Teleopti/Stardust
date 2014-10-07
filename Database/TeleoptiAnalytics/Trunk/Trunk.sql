-- 2014-10-06 RK: Adding new view for log object details to make troubleshooting easier
/*
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] ON
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (7, N'v_log_object_detail', N'SELECT * FROM [$$$target$$$].dbo.log_object_detail', 4)
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] OFF
GO
*/

--Name: Team Green
--Date: 2014-10-07
--Desc: Removed the ActualAgentState history table
IF OBJECT_ID('RTA.ActualAgentState_History') IS NOT NULL
BEGIN
	DROP TABLE RTA.ActualAgentState_History
END
GO

-------------
--PBI #30787 - new tables to store last agg date and interval
-------------
--implemented in code instead
DROP TABLE [mart].[sys_crossdatabaseview]

--new tables
CREATE TABLE [mart].[sys_datasource_detail_type](
	[detail_id] int NOT NULL,
	[detail_desc] varchar(50) NOT NULL,
	CONSTRAINT [PK_log_object_detail_type] PRIMARY KEY CLUSTERED 
(
	[detail_id] ASC
)
)
INSERT INTO [mart].[sys_datasource_detail_type]
SELECT 1,'Agent'
UNION ALL
SELECT 2,'Queue'

--drop table [mart].[sys_datasource_detail]
CREATE TABLE [mart].[sys_datasource_detail](
	[datasource_id] smallint NOT NULL,
	[detail_id] int NOT NULL,
	[target_date_local] smalldatetime NOT NULL,
	[target_interval_local] smallint NOT NULL,
	[intervals_back] smallint NOT NULL
	CONSTRAINT [PK_log_object_detail] PRIMARY KEY CLUSTERED 
(
	[datasource_id] ASC,
	[detail_id] ASC
)
)

ALTER TABLE [mart].[sys_datasource_detail]  WITH CHECK ADD  CONSTRAINT [FK_sys_datasource_detail_sys_datasource] FOREIGN KEY([datasource_id])
REFERENCES [mart].[sys_datasource] ([datasource_id])

ALTER TABLE [mart].[sys_datasource_detail] CHECK CONSTRAINT [FK_sys_datasource_detail_sys_datasource]

ALTER TABLE [mart].[sys_datasource_detail]  WITH CHECK ADD  CONSTRAINT [FK_sys_datasource_detail_sys_datasource_detail_type] FOREIGN KEY([detail_id])
REFERENCES [mart].[sys_datasource_detail_type] ([detail_id])

ALTER TABLE [mart].[sys_datasource_detail] CHECK CONSTRAINT [FK_sys_datasource_detail_sys_datasource_detail_type]
