/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE mart.Tmp_dim_queue_excluded
	(
	queue_original_id nvarchar(100) NOT NULL,
	datasource_id smallint NOT NULL
	)  ON MART
GO
ALTER TABLE mart.Tmp_dim_queue_excluded SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM mart.dim_queue_excluded)
	 EXEC('INSERT INTO mart.Tmp_dim_queue_excluded (queue_original_id, datasource_id)
		SELECT CONVERT(nvarchar(100), queue_original_id), datasource_id FROM mart.dim_queue_excluded WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE mart.dim_queue_excluded
GO
EXECUTE sp_rename N'mart.Tmp_dim_queue_excluded', N'dim_queue_excluded', 'OBJECT' 
GO
ALTER TABLE mart.dim_queue_excluded ADD CONSTRAINT
	PK_dim_queue_excluded PRIMARY KEY CLUSTERED 
	(
	queue_original_id,
	datasource_id
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
COMMIT
