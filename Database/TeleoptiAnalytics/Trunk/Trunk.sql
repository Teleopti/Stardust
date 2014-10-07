-- 2014-10-06 RK: Adding new view for log object details to make troubleshooting easier

SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] ON
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (7, N'v_log_object_detail', N'SELECT * FROM [$$$target$$$].dbo.log_object_detail', 4)
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] OFF
GO

EXEC mart.sys_crossdatabaseview_load
GO