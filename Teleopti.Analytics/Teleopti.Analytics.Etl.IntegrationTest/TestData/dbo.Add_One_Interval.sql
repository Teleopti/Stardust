create proc dbo.Add_One_Interval
@agent_id int,
@agent_name nvarchar(50),
@queue_id int,
@addMinute int,
@testDate datetime = null,
@interval int = null

AS
declare @Minute int
declare @maxdate smalldatetime
declare @maxinterval int

set @Minute = 60 --seconds

if @testDate is null
select @testDate=isnull(max(date_from),'2000-01-01') from dbo.agent_logg

if @interval is null
begin
	select @interval=isnull(max(interval),0)+1 from dbo.agent_logg where date_from=@testDate
	if @interval=96
	begin
		set @interval=0
		set @testDate=dateadd(dd,1,@testDate)
	end
end

--@testDate, used for n last intervals
delete from dbo.agent_logg
WHERE QUEUE=@queue_id
	AND	date_from=@testDate
	AND interval=@interval

insert dbo.agent_logg
values (@queue_id,@testDate,@interval,@agent_id,@agent_name,@Minute*@addMinute,@Minute*@addMinute,@Minute*@addMinute,0,0,0,0,0,0,0,0,0,NULL)

--if this is a newer interval than previous inser, update dbo.log_object_detail
update lod
set
	lod.date_value	= @testDate,
	lod.int_value	= @interval
from dbo.log_object_detail lod
where lod.detail_id	= 2 --agent