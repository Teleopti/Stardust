
----------------  
--Name: Xianwei Shen
--Date: 2012-08-21
--Desc: Add options for whether contract time should come from contract of schedule period
----------------  	
ALTER TABLE dbo.Contract ADD
	IsWorkTimeFromContract int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromContract DEFAULT 1,
	IsWorkTimeFromSchedulePeriod int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromSchedulePeriod DEFAULT 0
GO

----------------  
--Name: Asad MIrza
--Date: 2012-08-29
--Desc: Added an extra column in activity to indicate if we can overwrite it or not
---------------- 
ALTER TABLE dbo.Activity ADD
	AllowOverwrite bit NOT NULL CONSTRAINT DF_Activity_AllowOverwrite DEFAULT 1
GO

ALTER TABLE dbo.Activity SET (LOCK_ESCALATION = TABLE)
GO
