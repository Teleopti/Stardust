IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Add_QueueAgent_stat') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.Add_QueueAgent_stat
GO

create proc dbo.Add_QueueAgent_stat
@TestDay datetime = null
as

set nocount on

If @TestDay is null set @TestDay=getdate()

declare @todayMinus2 smalldatetime
declare @todayMinus1 smalldatetime
declare @today smalldatetime
declare @todayPlus1 smalldatetime

declare @PaIdTodayMinus2 uniqueidentifier
declare @PaIdTodayMinus1 uniqueidentifier
declare @PaIdToday uniqueidentifier
declare @PaIdTodayPlus1 uniqueidentifier

declare @teamLondonPreferences uniqueidentifier
declare @teamParisNights uniqueidentifier
declare @currentPersonPeriodId uniqueidentifier
declare @NewPersonPeriodId uniqueidentifier

SELECT @todayMinus2 = DATEADD(dd, -2, DATEDIFF(dd, 0, GETDATE())) --day before yesterday
SELECT @todayMinus1 = DATEADD(dd, -1, DATEDIFF(dd, 0, GETDATE())) --yesterday
set @today = dateAdd(dd,1,@todayMinus1) --today (early this morning)
set @todayPlus1 = dateAdd(dd,1,@today) --tomorrow

--add cti data
truncate table dbo.agent_logg
DELETE from dbo.agent_info

truncate table dbo.queue_logg
delete from dbo.queues

declare @agent_id int
declare @agent_name nvarchar(50)
declare @queue_id int

set @queue_id = 11
set @agent_id =52
set @agent_name = 'Ashley Andeen'

declare @ExcelFrame int
set @ExcelFrame = 180 --seconds

delete from dbo.agent_info where agent_id = @agent_id
delete from dbo.agent_logg where agent_id = @agent_id

declare @log_object_id int
select @log_object_id = max(log_object_id) from dbo.log_object


SET IDENTITY_INSERT dbo.agent_info ON
insert into dbo.agent_info(Agent_id,Agent_name,is_active,log_object_id,orig_agent_id)
select @agent_id,@agent_name,1,@log_object_id,@agent_id+100
SET IDENTITY_INSERT dbo.agent_info OFF

SET IDENTITY_INSERT dbo.queues ON
insert into dbo.queues(queue,orig_desc,log_object_id,orig_queue_id,display_desc)
select @queue_id,N'queue_orig_desc',@log_object_id,@queue_id+100,N'queue_display_desc'
SET IDENTITY_INSERT dbo.queues OFF


insert dbo.agent_logg values (@queue_id,@todayMinus2,85,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,86,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,87,@agent_id,@agent_name,@ExcelFrame*2,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,88,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,89,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,90,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,91,@agent_id,@agent_name,@ExcelFrame*2,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,92,@agent_id,@agent_name,@ExcelFrame*1,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,93,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,94,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,95,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus2,96,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,0,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,1,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,2,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,3,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,4,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,5,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,6,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,7,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,8,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,9,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,10,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,11,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,12,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,13,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,14,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,15,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,16,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,17,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,18,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,19,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,20,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,21,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,22,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,23,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,24,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,25,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,26,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,27,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,28,@agent_id,@agent_name,@ExcelFrame*2,180,180,0,0,0,2,0,0,0,0,0,NULL)

insert dbo.agent_logg values (@queue_id,@todayMinus1,55,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,56,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,57,@agent_id,@agent_name,@ExcelFrame*2,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,58,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,59,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,60,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,61,@agent_id,@agent_name,@ExcelFrame*2,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,62,@agent_id,@agent_name,@ExcelFrame*1,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,63,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,64,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,65,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,66,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,67,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)

insert dbo.agent_logg values (@queue_id,@todayMinus1,93,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,94,@agent_id,@agent_name,@ExcelFrame*4,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,95,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@todayMinus1,96,@agent_id,@agent_name,@ExcelFrame*2,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@today,0,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@today,1,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@today,2,@agent_id,@agent_name,@ExcelFrame*5,180,180,0,0,0,2,0,0,0,0,0,NULL)
insert dbo.agent_logg values (@queue_id,@today,3,@agent_id,@agent_name,@ExcelFrame*3,180,180,0,0,0,2,0,0,0,0,0,NULL)
