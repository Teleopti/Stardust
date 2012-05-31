
---------------------------
---- TO BE REMOVED --------
-- Below is only o debug/develop load SPs + CUBE

-- Try fix some data
---------------------------
truncate table [dbo].[quality_logg]
delete from  [dbo].[quality_info]
delete from [dbo].[log_object]

declare @acd_type_id int
declare @acd_type_desc varchar(50)

set @acd_type_id=25
set @acd_type_desc='Zoom QM'

declare @log_object_id int
select @log_object_id =  ISNULL(MAX(log_object_id)+10,10) from [dbo].[log_object]

INSERT INTO [dbo].[log_object]
           ([log_object_id]
           ,[acd_type_id]
           ,[log_object_desc]
           ,[logDB_name]
           ,[intervals_per_day]
           ,[default_service_level_sec]
           ,[default_short_call_treshold])
     VALUES
           (
           @log_object_id,
           @acd_type_id,
           'My Zoom Log',
           'LogDB',
           96,
           80,
           20
           )


INSERT INTO [dbo].[quality_info]
(
   [quality_name]
   ,[quality_type]
   ,[score_weight]
   ,[log_object_id]
   ,[original_id]
)
SELECT DISTINCT
   [quality_name] = q.qformname,
   [quality_type] = q.scoring_system,
   [score_weight] = q.report_weight,
   [log_object_id] = @log_object_id,
   [original_id] = q.qformid
from zoomlog.dbo.questforms q
inner join zoomlog.dbo.evaluations e
	on e.qformid = q.qformid

INSERT INTO [dbo].[quality_logg]
           ([quality_id],
           [date_from],
           [evaluation_id],
           [agent_id],
           [score]
           )
SELECT 
	quality_id	= qi.quality_id,
	date_from	= CONVERT(varchar(20),e.evaluation_date,112),
	evaluation_id = e.evaluationid,
	agent_id	= u.userid,
	score		= e.score
from zoomlog.dbo.evaluations e
inner join zoomlog.dbo.sc_users u on e.evaluated_user = u.userid
inner join zoomlog.dbo.questforms q on e.qformid = q.qformid
inner join  [dbo].[quality_info] qi on qi.original_id = q.qformid
where e.evalstatus = 'FINISHED'
and u.status <> 'DELETED'

EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', 'PBI9112_Demoreg_TeleoptiCCCAgg'
EXEC mart.sys_crossDatabaseView_load

--Run initial load from ETL, then run
/*
--Make it a internal log_object:
update mart.sys_datasource
set internal=1
where log_object_name='My Zoom Log'

--The stuff:
exec mart.etl_dim_quality_quest_type_load -2
exec mart.etl_dim_quality_quest_load -2

--check the result:
select * from mart.dim_quality_quest_type
select * from mart.dim_quality_quest

*/
