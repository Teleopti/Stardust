IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_log_save_init]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_log_save_init]
GO


CREATE PROCEDURE [mart].[etl_log_save_init]
 
  
AS
BEGIN

	SET NOCOUNT ON;
    INSERT INTO Mart.etl_job_execution (insert_date)
    VALUES (default)

    declare @id as int;
	set @id = SCOPE_IDENTITY();

    select @id;

END


GO

