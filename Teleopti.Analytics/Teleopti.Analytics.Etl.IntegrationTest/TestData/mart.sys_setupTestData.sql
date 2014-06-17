--drop proc mart.sys_setupTestData
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

truncate table dbo.queue_logg
truncate table dbo.agent_logg

delete from dbo.queues
delete from dbo.agent_info
delete from dbo.log_object
delete from dbo.acd_type
delete from dbo.ccc_system_info

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
where datasource_name = 'Teleopti WFM Agg: Default log object'

exec mart.sys_datasource_set_raptor_time_zone
