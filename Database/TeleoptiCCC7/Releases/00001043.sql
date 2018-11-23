alter table planninggroup add PreferenceValue float
GO
update planninggroup set PreferenceValue = 1
alter table planninggroup alter column PreferenceValue float not null