alter table planninggroup add PreferenceValue float
update planninggroup set PreferenceValue = 1
alter table planninggroup alter column PreferenceValue float not null
