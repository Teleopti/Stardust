IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_web_result]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_web_result]
GO

/*
exec mart.report_data_agent_schedule_web_result 
@date_from='2013-02-05 00:00:00',
@date_to='2013-02-07 00:00:00',
@interval_from=N'0',
@interval_to=N'95',
@adherence_id=N'1',
@time_zone_id=N'2',
@person_code='11610fe4-0130-4568-97de-9b5e015b2564',
@language_id=103,
@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
*/

CREATE PROCEDURE [mart].[report_data_agent_schedule_web_result] 
@date_from datetime,
@date_to datetime,
@interval_from int,
@interval_to int,
@adherence_id int,
@time_zone_id int,
@person_code uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
as
set nocount on

exec mart.report_data_agent_schedule_result 
@date_from=@date_from,
@date_to=@date_to,
@interval_from=@interval_from,
@interval_to=@interval_to,
@group_page_code=null,
@group_page_group_set=NULL,
@group_page_agent_set=NULL,
@site_id=null,
@team_set=null,
@agent_set=null,
@adherence_id=@adherence_id,
@time_zone_id=@time_zone_id,
@person_code=@person_code,
@report_id=null,
@language_id=@language_id,
@business_unit_code=@business_unit_code, 
@from_matrix=0

go