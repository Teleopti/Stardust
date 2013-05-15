IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_availability_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_availability_per_agent]
GO


create PROCEDURE [mart].[report_data_availability_per_agent]
@scenario_id int,
@date_from datetime,
@date_to datetime,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier,
@site_id int,
@team_set nvarchar(max),
@agent_code uniqueidentifier,
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    CREATE TABLE #RESULT(PersonName nvarchar(500),
					AvailableDays int,
					AvailableMinutes int,
					ScheduledDays int,
					ScheduledMinutes int)
					
	INSERT INTO #RESULT values('Ola Håkansson',3, 580, 2, 200 )
	INSERT INTO #RESULT values('Karin Jeppson',5, 1200, 4, 800 )
	INSERT INTO #RESULT values('David Johnsson',12, 2500, 3, 200 )
	INSERT INTO #RESULT values('Erik Sundberg',10, 1600, 10, 1600 )

	SELECT * FROM #RESULT
END


GO

