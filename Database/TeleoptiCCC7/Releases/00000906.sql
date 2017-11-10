ALTER TABLE OvertimeRequestOpenPeriod ADD  EnableWorkRuleValidation bit NOT NULL Default(0)
GO
ALTER TABLE OvertimeRequestOpenPeriod ADD  WorkRuleValidationHandleType int
GO