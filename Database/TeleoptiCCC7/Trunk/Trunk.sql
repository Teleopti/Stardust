
----------------  
--Name: Xianwei Shen
--Date: 2012-08-21
--Desc: Add options for whether contract time should come from contract of schedule period
----------------  	
ALTER TABLE dbo.Contract ADD
	IsWorkTimeFromContract int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromContract DEFAULT 1,
	IsWorkTimeFromSchedulePeriod int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromSchedulePeriod DEFAULT 0
GO