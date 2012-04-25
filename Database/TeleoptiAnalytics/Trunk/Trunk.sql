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
ALTER TABLE stage.stg_person ADD
	windows_domain		nvarchar(50) NULL,
	windows_username	nvarchar(50) NULL

--mart	
--===============================
--Use if exist since PS Tech might have delivered this part already
--===============================
if not exists(select * from sys.columns where Name = N'windows_domain' and Object_ID = Object_ID(N'mart.dim_person')) 
begin 
	ALTER TABLE mart.dim_person ADD	windows_domain nvarchar(50) NULL
end
GO
if not exists(select * from sys.columns where Name = N'windows_domain' and Object_ID = Object_ID(N'mart.dim_person')) 
begin 
	UPDATE mart.dim_person SET windows_domain = 'Not Defined'
end	
GO
if not exists(select * from sys.columns where Name = N'windows_username' and Object_ID = Object_ID(N'mart.dim_person')) 
begin
	ALTER TABLE mart.dim_person ADD	windows_username nvarchar(50) NULL
end
GO
if not exists(select * from sys.columns where Name = N'windows_username' and Object_ID = Object_ID(N'mart.dim_person')) 
begin
	UPDATE mart.dim_person SET windows_username = 'Not Defined'
end
GO
--===============================

ALTER TABLE mart.dim_person ALTER COLUMN windows_domain		nvarchar(50) NOT NULL
ALTER TABLE mart.dim_person ALTER COLUMN windows_username	nvarchar(50) NOT NULL
GO
