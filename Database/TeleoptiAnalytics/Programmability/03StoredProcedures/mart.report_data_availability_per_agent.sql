IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_availability_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_availability_per_agent]
GO

-- exec mart.report_data_availability_per_agent @scenario_id=N'0',@date_from='2013-05-13 00:00:00',@date_to='2013-05-14 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'0',@team_set=N'7',@agent_code=N'00000000-0000-0000-0000-000000000002',@time_zone_id=N'2',@person_code='BABBBA8D-52D3-475B-85DD-FE307C290522',@report_id='A56B3EEF-17A2-4778-AA8A-D166232073D2',@language_id=1053,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'


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

    CREATE TABLE #RESULT(PersonName nvarchar(50),
					AvailableDays int,
					AvailableMinutes int,
					ScheduledDays int,
					ScheduledMinutes int,
					hide_time_zone int)
					
	INSERT INTO #RESULT values('Ola Håkansson',3, 580, 2, 200,1 )
	INSERT INTO #RESULT values('Karin Jeppson',5, 1200, 4, 800,1 )
	INSERT INTO #RESULT values('David Johnsson',12, 2500, 3, 200,1 )
	INSERT INTO #RESULT values('Erik Sundberg',10, 1600, 10, 1600,1 )

	SELECT * FROM #RESULT
END


GO

