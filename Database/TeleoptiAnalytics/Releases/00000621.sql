
---------------------------------------------------------------------------------
-- 		Bug 75992: Upgrade fails when trying to create index on stage.stg_queue 
IF EXISTS (
			SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
			WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' 
			AND CONSTRAINT_NAME = 'PK_stg_queue'
			AND TABLE_NAME = 'stg_queue' 
			AND TABLE_SCHEMA ='stage'
		)
BEGIN
	ALTER TABLE stage.stg_queue	DROP CONSTRAINT PK_stg_queue
END 
GO


