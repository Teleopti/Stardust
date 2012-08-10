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