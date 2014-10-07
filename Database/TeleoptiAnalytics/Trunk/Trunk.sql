

-- 2014-10-06 RK: Adding new view for log object details to make troubleshooting easier

SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] ON
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (7, N'v_log_object_detail', N'SELECT * FROM [$$$target$$$].dbo.log_object_detail', 4)
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] OFF
GO

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
