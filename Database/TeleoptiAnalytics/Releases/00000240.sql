/* 
Trunk initiated: 
2010-05-04 
08:40
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: jonas n
--Date: 2010-05-04  
--Desc: Add inner exception columns to ETL error log  
----------------  
ALTER TABLE mart.etl_jobstep_error
	DROP COLUMN error_exception_inner
GO



ALTER TABLE mart.etl_jobstep_error
	DROP CONSTRAINT DF_etl_jobstep_error_insert_date
GO
ALTER TABLE mart.etl_jobstep_error
	DROP CONSTRAINT DF_etl_jobstep_error_update_date
GO
CREATE TABLE mart.Tmp_etl_jobstep_error
	(
	jobstep_error_id int NOT NULL IDENTITY (1, 1),
	error_exception_message text NULL,
	error_exception_stacktrace text NULL,
	inner_error_exception_message text NULL,
	inner_error_exception_stacktrace text NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL
	)  ON MART
	 TEXTIMAGE_ON MART
GO
ALTER TABLE mart.Tmp_etl_jobstep_error ADD CONSTRAINT
	DF_etl_jobstep_error_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_etl_jobstep_error ADD CONSTRAINT
	DF_etl_jobstep_error_update_date DEFAULT (getdate()) FOR update_date
GO
SET IDENTITY_INSERT mart.Tmp_etl_jobstep_error ON
GO
IF EXISTS(SELECT * FROM mart.etl_jobstep_error)
	 EXEC('INSERT INTO mart.Tmp_etl_jobstep_error (jobstep_error_id, error_exception_message, error_exception_stacktrace, insert_date, update_date)
		SELECT jobstep_error_id, error_exception_message, error_exception_stacktrace, insert_date, update_date FROM mart.etl_jobstep_error WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT mart.Tmp_etl_jobstep_error OFF
GO
ALTER TABLE mart.etl_jobstep_execution
	DROP CONSTRAINT FK_etl_jobstep_execution_etl_jobstep_error
GO
DROP TABLE mart.etl_jobstep_error
GO
EXECUTE sp_rename N'mart.Tmp_etl_jobstep_error', N'etl_jobstep_error', 'OBJECT' 
GO
ALTER TABLE mart.etl_jobstep_error ADD CONSTRAINT
	PK_etl_jobstep_error PRIMARY KEY CLUSTERED 
	(
	jobstep_error_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO

ALTER TABLE mart.etl_jobstep_execution ADD CONSTRAINT
	FK_etl_jobstep_execution_etl_jobstep_error FOREIGN KEY
	(
	jobstep_error_id
	) REFERENCES mart.etl_jobstep_error
	(
	jobstep_error_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (240,'7.1.240') 
