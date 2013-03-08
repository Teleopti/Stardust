IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_execute_delayed_job]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_execute_delayed_job]
GO
-- =============================================
-- Author:		DJ
-- Description:	Picks oldest delayed job and executes it
-- Updates:		2013-xx-yy some comment
-- =============================================
CREATE PROCEDURE [mart].[etl_execute_delayed_job]
@stored_procedure nvarchar(300) = null
WITH EXECUTE AS OWNER
AS

declare @sqlcommand nvarchar(4000)
declare @Id int

if @stored_procedure is null
	SELECT top 1 @Id=Id, @sqlcommand = stored_procedured + ' ' + parameter_string + ', @is_delayed_job=1'
	from mart.etl_job_delayed --unspecified SP
	order by Id

else
	SELECT top 1 @Id=Id,@sqlcommand = stored_procedured + ' ' + parameter_string + ', @is_delayed_job=1'
	from mart.etl_job_delayed
	where stored_procedured = @stored_procedure --specified SP
	order by Id

BEGIN TRY
	update mart.etl_job_delayed
	set execute_date = getdate()
	where Id=@Id

	exec sp_executesql @sqlcommand

	delete from mart.etl_job_delayed
	where Id=@Id
END TRY
BEGIN CATCH
  DECLARE @ErrMsg nvarchar(4000)
  DECLARE @ErrSeverity int
  SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
  RAISERROR(@ErrMsg, @ErrSeverity, 1)
END CATCH