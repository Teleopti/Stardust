declare @sql nvarchar(1000)

DECLARE @AppDb nvarchar(260)
SET @AppDb=N'$(APPDB)'
DECLARE @AnalDb nvarchar(260)
SET @AnalDb=N'$(ANALDB)'

set @sql='insert into [' + @AnalDb + '].mart.dim_scenario 
(scenario_code, scenario_name, default_scenario, business_unit_id, business_unit_name, datasource_id, insert_date, update_date,is_deleted)
values 
((select id from [' + @AppDb + '].dbo.scenario where defaultscenario=1), ''name'', 1, -1, ''name'', 1, getdate(), getdate(), 0)'

exec sp_executesql @sql


select * from perfanal.mart.dim_scenario
