IF NOT EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE  TABLE_NAME = 'PersonAssignment'
                 AND COLUMN_NAME = 'Source') 
BEGIN
	ALTER TABLE [dbo].[PersonAssignment] 
	ADD [Source] nvarchar(50) NULL
END
