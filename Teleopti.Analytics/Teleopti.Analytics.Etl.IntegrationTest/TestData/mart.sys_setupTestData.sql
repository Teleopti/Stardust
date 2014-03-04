create proc mart.sys_setupTestData
@IntervalsPerDay INT = 96,
@Culture nvarchar(200) = N'1053',
@TimeZoneCode nvarchar(200) = N'W. Europe Standard Time',
@time_zone_name nvarchar(100) = N'(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna',
@utc_conversion int = 60,
@utc_conversion_dst int = 120,
@AdherenceMinutesOutsideShift nvarchar(200) = N'120'
AS
SET NOCOUNT ON

exec mart.etl_data_mart_delete 1
truncate table dbo.queue_logg
truncate table dbo.agent_logg

delete from dbo.queues
delete from dbo.agent_info

truncate table mart.sys_configuration
delete from [dbo].[log_object]
delete from dbo.acd_type
delete from dbo.ccc_system_info

DECLARE @IntervalLengthMinutes INT
set @IntervalLengthMinutes=1440/@IntervalsPerDay

exec mart.sys_configuration_save @key=N'Culture',@value=@Culture
exec mart.sys_configuration_save @key=N'IntervalLengthMinutes',@value=@IntervalLengthMinutes
exec mart.sys_configuration_save @key=N'TimeZoneCode',@value=@TimeZoneCode
exec mart.sys_configuration_save @key=N'AdherenceMinutesOutsideShift',@value=@AdherenceMinutesOutsideShift

--dim_interval
delete from mart.dim_interval
insert into mart.dim_interval(interval_id,interval_name, halfhour_name, hour_name, interval_start, interval_end, datasource_id, insert_date, update_date)
select
	n,
	CONVERT(varchar(5),dateadd(MINUTE,@IntervalLengthMinutes*n,'1900-01-01 00:00:00'),114) + '-' + CONVERT(varchar(5),dateadd(MINUTE,@IntervalLengthMinutes*(n+1),'1900-01-01 00:00:00'),114) as 'interval_name',
	CONVERT(varchar(5),dateadd(MINUTE,@IntervalLengthMinutes*n,'1900-01-01 00:00:00'),114) + '-' + CONVERT(varchar(5),dateadd(MINUTE,@IntervalLengthMinutes*(n)+30,'1900-01-01 00:00:00'),114) as 'halfhour_name',
	CONVERT(varchar(5),dateadd(MINUTE,@IntervalLengthMinutes*n,'1900-01-01 00:00:00'),114) + '-' + CONVERT(varchar(5),dateadd(MINUTE,@IntervalLengthMinutes*(n)+60,'1900-01-01 00:00:00'),114) as 'hour_name',
	dateadd(MINUTE,@IntervalLengthMinutes*(n),'1900-01-01 00:00:00') as 'interval_start',
	dateadd(MINUTE,@IntervalLengthMinutes*(n+1),'1900-01-01 00:00:00') as 'interval_end',
	1 as 'datasource_id',
	getdate() as 'insert_date',
	getdate() as 'update_date'
FROM mart.sys_numbers n
where n<@IntervalsPerDay

--dim_date
delete mart.dim_date
truncate table stage.stg_date
exec mart.etl_dim_date_load
--select * from mart.dim_date;select * from mart.dim_interval;select * from mart.sys_datasource

--dim_timezone
truncate table mart.bridge_time_zone
delete from mart.dim_time_zone

SET IDENTITY_INSERT mart.dim_time_zone ON 
insert into mart.dim_time_zone (time_zone_id, time_zone_code, time_zone_name, default_zone, utc_conversion, utc_conversion_dst, datasource_id, insert_date, update_date, to_be_deleted)
select 1,N'UTC',N'UTC',0,0,0,-1,getdate(),getdate(),-1
union all
select 2,@TimeZoneCode,@time_zone_name,1,@utc_conversion,@utc_conversion_dst,-1,getdate(),getdate(),-1
SET IDENTITY_INSERT mart.dim_time_zone OFF

----------------  
--Name: Anders F  
--Date: 2009-03-20  
--Desc: Correct initial data for new aggs  
----------------  
INSERT INTO acd_type
VALUES (0,'Default')

INSERT INTO acd_type
VALUES (1,'Avaya Definity CMS')

INSERT INTO acd_type
VALUES (2,'Nortel Symposium 3-4.0 Skillset')

INSERT INTO acd_type
VALUES (3,'Nortel Symposium 1.5 Skillset')

INSERT INTO acd_type
VALUES (4,'Nortel Symposium 3-4.0 Application')

INSERT INTO acd_type
VALUES (5,'Nortel Symposium 1.5 Application')

INSERT INTO acd_type
VALUES (6,'Siemens ProCenter Advanced')

INSERT INTO acd_type
VALUES (7,'Siemens ProCenter Entry')

INSERT INTO acd_type
VALUES (8,'Ericsson Solidus E-Care')

INSERT INTO acd_type
VALUES (9,'Ericsson CCM')

INSERT INTO acd_type
VALUES (10,'Interactive Intelligence Interaction center')

INSERT INTO acd_type
VALUES (11,'Telia VCC 7.5')

INSERT INTO acd_type
VALUES (12,'ClearIT MCC')

INSERT INTO acd_type
VALUES (13,'WebTrump - ccBridge')

INSERT INTO acd_type
VALUES (14,'Nokia DX200')

INSERT INTO acd_type
VALUES (15,'Cisco ICM5')

INSERT INTO acd_type
VALUES (16,'Advoco')

INSERT INTO acd_type
VALUES (17,'TeliaSonera CallGuide 4')

INSERT INTO acd_type
VALUES (18,'Alcatel')

INSERT INTO acd_type
VALUES (19,'Telia VCC 7.6')

INSERT INTO acd_type
VALUES (20,'Telia VCC 8')

INSERT INTO acd_type
VALUES (21,'CDS')

INSERT INTO acd_type
VALUES (22,'Wicom')

INSERT INTO acd_type
VALUES (23,'Altitude 6.2')

INSERT INTO acd_type
VALUES (24,'Wicomrt')

INSERT INTO acd_type
VALUES (25,'Zoom QM')

INSERT INTO acd_type
VALUES (26,'Globitel Speechlog')

--intervals = 96
INSERT INTO ccc_system_info
VALUES (1, 'CCC intervals per day', @IntervalsPerDay,NULL)

--A dummy log object
INSERT INTO [dbo].[log_object]
           ([log_object_id]
           ,[acd_type_id]
           ,[log_object_desc]
           ,[logDB_name]
           ,[intervals_per_day]
           ,[default_service_level_sec]
           ,[default_short_call_treshold])
     VALUES
           (1
           ,1
           ,'Default log object'
           ,db_name()
           ,@IntervalsPerDay
           ,20
           ,5)


exec mart.sys_datasource_load
exec [mart].[sys_datasource_set_raptor_time_zone]

declare @time_zone_id int
select @time_zone_id = time_zone_id
from mart.dim_time_zone
where time_zone_code = @TimeZoneCode

update mart.sys_datasource
set time_zone_id = @time_zone_id
where datasource_name = 'Teleopti CCC Agg: Default log object'

exec mart.sys_datasource_set_raptor_time_zone
