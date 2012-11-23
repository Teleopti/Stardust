--Robin: Changing behavior of ETL locking

if not exists(select * from sys.columns where Name = N'lock_until' and Object_ID = Object_ID(N'mart.sys_etl_running_lock')) 
begin 
ALTER TABLE mart.sys_etl_running_lock ADD
	lock_until datetime NOT NULL CONSTRAINT DF_sys_etl_running_lock_lock_until DEFAULT dateadd(mi,1,getutcdate())
end
GO

-- Jonas: Standard reports. Change report control type for interval from and to.
IF EXISTS (select 1 from mart.report_control where control_id = 12 and control_name like 'cbo%')
	UPDATE mart.report_control SET control_name = REPLACE(control_name, 'cbo', 'time') WHERE control_id = 12
IF EXISTS (select 1 from mart.report_control where control_id = 13 and control_name like 'cbo%')
	UPDATE mart.report_control SET control_name = REPLACE(control_name, 'cbo', 'time') WHERE control_id = 13

----------------  
--Name: David and Erik
--Date: 2012-11-23  
--Desc: Bug #21451 Speed up intraday
----------------  	
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_person_dateFrom_dateTo')
DROP INDEX [IX_person_dateFrom_dateTo] ON [mart].[dim_person]
GO

CREATE NONCLUSTERED INDEX [IX_person_dateFrom_dateTo]
ON [mart].[dim_person] ([valid_from_date],[valid_to_date])
INCLUDE ([person_id],[skillset_id])
GO
