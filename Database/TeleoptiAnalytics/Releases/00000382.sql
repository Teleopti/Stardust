----------------  
--Name: CS
--Date: 2013-02-11  
--Desc: PBI #22024 
----------------  

-- Add column for activity preference

ALTER TABLE stage.stg_schedule_preference ADD
	activity_code uniqueidentifier NULL
GO


----------------  
--Name: Karin and David
--Date: 2013-03-04
--Desc: #22446
---------------- 
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_delayed]') AND type in (N'U'))
BEGIN
	CREATE TABLE mart.etl_job_delayed
		(
			Id int identity (1,1) not null,
			stored_procedured nvarchar(300) not null,
			parameter_string nvarchar(1000) not null,
			insert_date smalldatetime not null,
			execute_date smalldatetime null
		)
	ALTER TABLE mart.etl_job_delayed ADD CONSTRAINT
		PK_etl_job_delayed PRIMARY KEY CLUSTERED 
		(
		Id
		)
	ALTER TABLE [mart].[etl_job_delayed] ADD  CONSTRAINT [DF_etl_job_delayed_insert_date]  DEFAULT (getdate()) FOR [insert_date]
END
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (382,'7.3.382') 
