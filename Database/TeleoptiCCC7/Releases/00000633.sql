CREATE UNIQUE NONCLUSTERED INDEX [UQ_DayOffRules_DefaultSettings]
ON dbo.DayOffRules (DefaultSettings, BusinessUnit)
WHERE DefaultSettings=1
