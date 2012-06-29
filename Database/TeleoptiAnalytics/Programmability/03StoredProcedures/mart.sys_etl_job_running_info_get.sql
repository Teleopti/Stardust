/****** Object:  StoredProcedure [mart].[sys_etl_job_running_info_get]    Script Date: 06/29/2012 13:40:41 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_etl_job_running_info_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_etl_job_running_info_get]
GO

-- =============================================
-- Author:		Jonas N
-- Create date: 2012-04-04
-- Description:	Should do dirty read from lock table and get info about who is running a ETL job right now.

-- Changed:		
-- =============================================
CREATE PROCEDURE [mart].[sys_etl_job_running_info_get] 

AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [mart].[sys_etl_running_lock] WHERE lock_until<GETUTCDATE()
	SELECT 
		computer_name,
		start_time,
		job_name,
		is_started_by_service,
		lock_until
	FROM 
		[mart].[sys_etl_running_lock]
	WHERE lock_until>GETUTCDATE()
END



GO


