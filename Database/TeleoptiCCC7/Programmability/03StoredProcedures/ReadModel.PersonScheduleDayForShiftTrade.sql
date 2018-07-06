-- =============================================
-- Author:		Mingdi
-- Create date: 2018-07-05
-- Description:	Load me and personTo Schedules for shift trades within a period
-- =============================================

--exec ReadModel.PersonScheduleDayForShiftTrade 
--@start_date='2012-08-29',@end_date='2012-08-29 23:00:00',@my_person_id = '2C24C166-3940-4487-B8A0-8BC449A10926',
--@person_to_id = '0E8AA7D0-4235-45E8-81E2-D88F88FF3AF3',@is_my_dayoff = false,@is_person_dayoff = false

CREATE PROCEDURE ReadModel.PersonScheduleDayForShiftTrade
@start_date datetime,
@end_date datetime,
@my_person_id uniqueidentifier,
@person_to_id uniqueidentifier,
@is_my_dayoff bit,
@is_person_dayoff bit
AS
SET NOCOUNT ON 

create table #my_schedule (
	PersonId uniqueidentifier,
	[Date] datetime,
	Start datetime,
	[End] datetime,
	Model nvarchar(max)
)

create table #personTo_schedule (
	PersonId uniqueidentifier,
	[Date] datetime,
	Start datetime,
	[End] datetime,
	Model nvarchar(max)
)

create table #allSchedules (
	PersonId uniqueidentifier,
	[Date] datetime,
	Start datetime,
	[End] datetime,
	Model nvarchar(max)
)

insert into #my_schedule
select PersonId, BelongsToDate as [Date], Start, [End], Model 
					from ReadModel.PersonScheduleDay
					where PersonId =  @my_person_id AND Start IS NOT NULL AND Start < @end_date AND [End] > @start_date AND IsDayOff = @is_my_dayoff

insert into #personTo_schedule
select PersonId, BelongsToDate as [Date], Start, [End], Model 
					from ReadModel.PersonScheduleDay
					where PersonId =  @person_to_id AND Start IS NOT NULL AND Start < @end_date AND [End] > @start_date AND IsDayOff = @is_person_dayoff

insert into #allSchedules
select a.PersonId, a.[Date], a.Start, a.[End],a.Model from #my_schedule a 
inner join #personTo_schedule b
on a.[Date] = b.[Date]

insert into #allSchedules
select b.PersonId, b.[Date], b.Start, b.[End],b.Model from #personTo_schedule b
inner join #my_schedule a
on a.[Date] = b.[Date]

select * from #allSchedules
GO