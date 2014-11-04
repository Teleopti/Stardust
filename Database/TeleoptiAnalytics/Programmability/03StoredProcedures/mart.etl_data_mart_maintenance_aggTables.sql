IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_data_mart_maintenance_aggTables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_data_mart_maintenance_aggTables]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		David J
-- Create date: 2014-11-04
-- Description:	Avoid Agg tables when custom views
-- =============================================
CREATE PROCEDURE [mart].[etl_data_mart_maintenance_aggTables]
AS
BEGIN

	delete mart.v_agent_logg
	from mart.v_agent_logg f
	where 1=1
	and f.date_from < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 15
						and configuration_name = 'YearsToKeepAggAgentStats'),100),getdate())
	and f.date_from < (select dateadd(day,10,min(f2.date_from))
						from mart.v_agent_logg f2)

	delete mart.v_queue_logg
	from mart.v_queue_logg f
	where 1=1
	and f.date_from < dateadd(year,-1*isnull((select isnull(configuration_value,100) from [mart].[etl_maintenance_configuration] where configuration_id = 14
						and configuration_name = 'YearsToKeepAggQueueStats'),100),getdate())
	and f.date_from < (select dateadd(day,10,min(f2.date_from))
						from mart.v_queue_logg f2)
END

GO