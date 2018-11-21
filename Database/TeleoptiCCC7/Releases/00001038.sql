delete from planningperiod where planninggroup is null

alter table planningperiod
alter column planninggroup
uniqueidentifier not null



