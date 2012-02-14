IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_log_jobstep_save]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_log_jobstep_save]
GO


CREATE PROCEDURE [mart].[etl_log_jobstep_save]
	
	@job_execution_id int,
	@jobstep_name varchar(50),
	@business_unit_code uniqueidentifier,
	@business_unit_name nvarchar(100),
	@duration int,
	@affected_rows int,
	@exception_message text,
	@exception_stacktrace text,
	@inner_exception_message text,
	@inner_exception_stacktrace text
  
AS
BEGIN
	SET NOCOUNT ON;

	if (@exception_message IS NOT NULL)
	BEGIN 
		INSERT INTO Mart.etl_jobstep_error (error_exception_message, error_exception_stacktrace, inner_error_exception_message, inner_error_exception_stacktrace)
		VALUES (@exception_message, @exception_stacktrace, @inner_exception_message, @inner_exception_stacktrace)
	END

	declare @error_id as int;
	set @error_id = SCOPE_IDENTITY();

	

	declare @jobstep_id as varchar(50)
	select @jobstep_id=jobstep_id from Mart.etl_jobstep where jobstep_name = @jobstep_name


    INSERT INTO Mart.etl_jobstep_execution (job_execution_id, business_unit_code, business_unit_name, jobstep_id, duration_s, rows_affected, jobstep_error_id)
    VALUES (@job_execution_id, @business_unit_code, @business_unit_name, @jobstep_id, @duration, @affected_rows, @error_id)

END


GO

