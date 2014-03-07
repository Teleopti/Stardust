create proc dbo.Add_QueueAgent_stat
@TestDay datetime = null,
@agent_id int = 52,
@orig_agent_id int = 152,
@agent_name nvarchar(50) = 'Ashley Andeen'
--select * from mart.v_agent_logg
--exec mart.etl_fact_agent_load '2014-01-01','2014-02-01',2
--exec mart.etl_fact_schedule_deviation_load '2014-01-01','2014-02-01','80E3AABD-3329-4111-B647-A2C200A5A1BD'

as

set nocount on

If @TestDay is null set @TestDay=getdate()

declare @todayMinus2 smalldatetime
declare @todayMinus1 smalldatetime
declare @today smalldatetime
declare @todayPlus1 smalldatetime

SELECT @todayMinus2 = DATEADD(dd, -2, DATEDIFF(dd, 0, @TestDay)) --day before yesterday
SELECT @todayMinus1 = DATEADD(dd, -1, DATEDIFF(dd, 0, @TestDay)) --yesterday
SELECT @today = DATEADD(dd, 0, DATEDIFF(dd, 0, @TestDay)) --yesterday
SELECT @todayPlus1 = DATEADD(dd, 1, DATEDIFF(dd, 0, @TestDay)) --tomorrow

--add cti data
truncate table dbo.agent_logg
DELETE from dbo.agent_info

truncate table dbo.queue_logg
delete from dbo.queues

declare @Minute int
set @Minute = 60 --seconds

delete from dbo.agent_info
delete from dbo.agent_logg

declare @log_object_id int
select @log_object_id = isnull(max(log_object_id),1) from dbo.log_object

declare @queue_id int
declare @orig_queue_id int
declare @orig_desc nvarchar(50)
declare @queue_display_desc nvarchar(50)

set @queue_id = 11
set @orig_queue_id = 111
set @queue_display_desc = N'queue_orig_desc'
set @orig_desc = N'queue_orig_desc'

SET IDENTITY_INSERT dbo.agent_info ON
insert into dbo.agent_info(Agent_id,Agent_name,is_active,log_object_id,orig_agent_id)
select @agent_id,@agent_name,1,@log_object_id,@orig_agent_id
SET IDENTITY_INSERT dbo.agent_info OFF

SET IDENTITY_INSERT dbo.queues ON
insert into dbo.queues(queue,orig_desc,log_object_id,orig_queue_id,display_desc)
select @queue_id,@orig_desc,@log_object_id,@orig_queue_id,@queue_display_desc
SET IDENTITY_INSERT dbo.queues OFF

--@todayMinus2
insert dbo.agent_logg values (@queue_id,@todayMinus2,38,@agent_id,@agent_name,@Minute*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,39,@agent_id,@agent_name,@Minute*6,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,48,@agent_id,@agent_name,@Minute*9,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,49,@agent_id,@agent_name,@Minute*15,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,58,@agent_id,@agent_name,@Minute*12,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,59,@agent_id,@agent_name,@Minute*9,180,180,0,0,0,2,0,0,0,0,0,NULL)

insert dbo.agent_logg values (@queue_id,@todayMinus2,78,@agent_id,@agent_name,@Minute*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,79,@agent_id,@agent_name,@Minute*6,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,88,@agent_id,@agent_name,@Minute*9,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,89,@agent_id,@agent_name,@Minute*15,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,95,@agent_id,@agent_name,@Minute*12,180,180,0,0,0,2,0,0,0,0,0,NULL)

insert dbo.agent_logg values (@queue_id,@todayMinus1,1,@agent_id,@agent_name,@Minute*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,2,@agent_id,@agent_name,@Minute*6,180,180,0,0,0,2,0,0,0,0,0,NULL)

--@todayMinus1 - in the overlapping shift
insert dbo.agent_logg values (@queue_id,@todayMinus1,27,@agent_id,@agent_name,@Minute*12,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,28,@agent_id,@agent_name,@Minute*9,180,180,0,0,0,2,0,0,0,0,0,NULL)

--@todayMinus1
insert dbo.agent_logg values (@queue_id,@todayMinus1,38,@agent_id,@agent_name,@Minute*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,39,@agent_id,@agent_name,@Minute*6,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,48,@agent_id,@agent_name,@Minute*9,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,49,@agent_id,@agent_name,@Minute*15,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,56,@agent_id,@agent_name,@Minute*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,58,@agent_id,@agent_name,@Minute*12,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,59,@agent_id,@agent_name,@Minute*9,180,180,0,0,0,2,0,0,0,0,0,NULL)
