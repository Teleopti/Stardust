----------------  
--Name: Anders  
--Date: 2009-05-06  
--Desc: Because log objects are nice to have...  
----------------  
IF NOT EXISTS (select 1 from log_object)
BEGIN
	DECLARE @log_object_id int
	DECLARE @acd_type_id int
	DECLARE @log_object_desc varchar(50)
	DECLARE @logDB_name varchar(50)
	DECLARE @intervals_per_day int
	DECLARE @default_service_level_sec int
	DECLARE @default_short_call_treshold int

	SET @log_object_id			 = 1
	SET @acd_type_id				 = 1
	SET @log_object_desc			 = N'Avaya CMS'
	SET @logDB_name				 = N'Logdb'
	SET @intervals_per_day			 = 96
	SET @default_service_level_sec	 = 20
	SET @default_short_call_treshold	 = 5
	----------------------

	INSERT [dbo].[log_object]
	([log_object_id], [acd_type_id], [log_object_desc], [logDB_name], [intervals_per_day], [default_service_level_sec], [default_short_call_treshold])
	VALUES (@log_object_id, @acd_type_id, @log_object_desc,@logDB_name,@intervals_per_day, @default_service_level_sec, @default_service_level_sec)

	INSERT INTO [dbo].[log_object_detail]
           ([log_object_id]
           ,[detail_id]
           ,[detail_desc]
           ,[proc_name]
           ,[int_value]
           ,[date_value])
     VALUES
           (1
           ,1
           ,'Queue_logg'
           ,'p_cms_queue_logg'
           ,0
           ,'20090101')

	INSERT INTO [dbo].[log_object_detail]
           ([log_object_id]
           ,[detail_id]
           ,[detail_desc]
           ,[proc_name]
           ,[int_value]
           ,[date_value])
     VALUES
           (1
           ,2
           ,'Queue_logg'
           ,'p_cms_agent_logg'
           ,0
           ,'20090101')

	INSERT INTO [dbo].[log_object_detail]
           ([log_object_id]
           ,[detail_id]
           ,[detail_desc]
           ,[proc_name]
           ,[int_value]
           ,[date_value])
     VALUES
           (1
           ,3
           ,'Goal results'
           ,'p_cms_goal_results'
           ,0
           ,'20090101')

END
GO
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (100,'7.0.100') 
