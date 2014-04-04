IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_shift_cat_and_day_abs_per_day]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_shift_cat_and_day_abs_per_day]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2011-09-21
-- Last Updated:
--				2011-09-21 Implement as wrapper to the shared SP: [mart].[report_data_shift_cat_and_day_abs_per_agent] 
--				2011-10-24 Change paramaters @group_page_group_id and @teamd_id to 
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
--				@group_page_group_set and @team_set
-- Description:	Used by reports:
-- 1) select * from mart.report where report_id = 26
-- =============================================
--exec report_data_shift_cat_and_day_abs_per_agent @scenario_id=N'0',@date_from='2006-01-01 00:00:00:000',@date_to='2006-01-05 00:00:00:000',@site_id=N'-2',@team_id=N'-2',@agent_id=N'-2',@shift_category_set=N'1,30,35,13,27,24,22',@day_off_set=N'4',@absence_set=N'1,2,3,4,5,6',@time_zone_id=N'1',@person_code='CCA9770F-6483-4015-8761-9B430010EE0F',@report_id=19,@language_id=1053


CREATE PROCEDURE [mart].[report_data_shift_cat_and_day_abs_per_day] 
@scenario_id int,
@date_from datetime,
@date_to datetime,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier,
@site_id int,
@team_set nvarchar(max),
@agent_code uniqueidentifier,
@shift_category_set nvarchar(max),
@day_off_set nvarchar(max),
@absence_set nvarchar(max),
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS

--Call shared SP
EXEC [mart].[report_data_shift_cat_and_day_abs_per_agent]
@scenario_id		= @scenario_id,
@date_from			= @date_from,
@date_to			= @date_to,
@group_page_code	= @group_page_code,
@group_page_group_set = @group_page_group_set,
@group_page_agent_code = @group_page_agent_code,
@site_id			= @site_id,
@team_set			= @team_set,
@agent_code			= @agent_code,
@shift_category_set = @shift_category_set,
@day_off_set		= @day_off_set,
@absence_set		= @absence_set,
@person_code		= @person_code,
@report_id			= @report_id,
@language_id		= @language_id,
@business_unit_code = @business_unit_code


