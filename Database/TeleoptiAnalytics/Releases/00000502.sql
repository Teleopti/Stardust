----------------  
--Name: JN
--Date: 2015-01-08 
--Desc: Add a toggle to be used from ETL code to toggle Intraday fact_schedule or event driven fact_schedule
----------------  
INSERT INTO mart.sys_configuration ([key], [value])
	VALUES ('ETL_SpeedUpETL_30791', 'False')
GO
