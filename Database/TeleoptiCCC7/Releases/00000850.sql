IF NOT EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE  TABLE_NAME = 'PersonAssignment_AUD'
                 AND COLUMN_NAME = 'Source') 
BEGIN
	ALTER TABLE [Auditing].[PersonAssignment_AUD] 
	ADD [Source] nvarchar(50) NULL
END
