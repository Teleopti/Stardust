--Improve delete in: mart.etl_fact_schedule_load
--Adding on 358 and default => IF NOT EXISTS
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_Shift_startdate_id')
CREATE NONCLUSTERED INDEX [IX_fact_schedule_Shift_startdate_id]
ON [mart].[fact_schedule] ([shift_startdate_id])
INCLUDE ([schedule_date_id],[person_id],[interval_id],[activity_starttime],[scenario_id],[business_unit_id])
GO

----------------
--PBI 19157
--David J
--New hiararcy in the Cube: WindowsCredentials
----------------
--stage

if exists(select * from sys.columns where Name = N'windows_domain' and Object_ID = Object_ID(N'stage.stg_person')) 
begin 
	ALTER TABLE stage.stg_person DROP COLUMN windows_domain
end
GO
if exists(select * from sys.columns where Name = N'windows_username' and Object_ID = Object_ID(N'stage.stg_person')) 
begin
	ALTER TABLE stage.stg_person DROP COLUMN windows_username
end
GO

ALTER TABLE stage.stg_person ADD
	windows_domain		nvarchar(50) NULL,
	windows_username	nvarchar(50) NULL

--mart	
--===============================
--Use if exist since PS Tech might have delivered this part already
--===============================
if exists(select * from sys.columns where Name = N'windows_domain' and Object_ID = Object_ID(N'mart.dim_person')) 
begin 
	ALTER TABLE mart.dim_person DROP COLUMN windows_domain
end
GO
if exists(select * from sys.columns where Name = N'windows_username' and Object_ID = Object_ID(N'mart.dim_person')) 
begin
	ALTER TABLE mart.dim_person DROP COLUMN windows_username
end
GO
--===============================

ALTER TABLE mart.dim_person ADD
	windows_domain nvarchar(50) NULL
GO
UPDATE mart.dim_person SET windows_domain = 'Not Defined'

ALTER TABLE mart.dim_person ADD	
    windows_username nvarchar(50) NULL
GO
UPDATE mart.dim_person SET windows_username = 'Not Defined'

GO
ALTER TABLE mart.dim_person ALTER COLUMN windows_domain		nvarchar(50) NOT NULL
ALTER TABLE mart.dim_person ALTER COLUMN windows_username	nvarchar(50) NOT NULL
GO

-----------------  
---Name: Jonas N
---Date: 2012-04-04
---Desc: Add a table used by ETL service and ETL Tool. 
---			Before a ETL job is started a check is made if another ETL is running. 
---			When a job is started a transaction lock is set on this table.
-----------------

CREATE TABLE [mart].[sys_etl_running_lock](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[computer_name] [nvarchar](255) NOT NULL,
	[start_time] [datetime] NOT NULL,
	[job_name] [nvarchar](100) NOT NULL,
	[is_started_by_service] [bit] NOT NULL,
 CONSTRAINT [PK_sys_etl_running_lock] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
)
GO
GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (360,'7.1.360') 
