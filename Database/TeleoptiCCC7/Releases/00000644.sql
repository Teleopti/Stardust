EXEC dbo.sp_rename @objname = N'[dbo].[AlarmType]', @newname = N'RtaRule'
EXEC dbo.sp_rename @objname = N'[dbo].[StateGroupActivityAlarm].[AlarmType]', @newname = N'RtaRule', @objtype = N'COLUMN'
EXEC dbo.sp_rename @objname = N'[FK_StateGroupActivityAlarm_AlarmType]', @newname = N'FK_StateGroupActivityAlarm_RtaRule', @objtype = N'OBJECT'