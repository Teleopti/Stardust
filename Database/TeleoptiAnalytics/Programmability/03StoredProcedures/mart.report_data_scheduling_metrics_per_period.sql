IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_scheduling_metrics_per_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_scheduling_metrics_per_period]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		DJ
-- Create date: 2013-09-30
-- Description:	Get avrage figures for scheduling
-- =============================================

CREATE PROCEDURE [mart].[report_data_scheduling_metrics_per_period]
@scheduling_type_id int,
@date_from datetime,
@date_to datetime,
@interval_type int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
BEGIN
SET NOCOUNT ON;
--exec mart.report_data_scheduling_metrics_per_period @scheduling_type_id=N'1',@date_from='2013-09-09 00:00:00',@date_to='2013-09-30 00:00:00',@interval_type=N'7',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id='F7F3AF97-EC24-4EA8-A2C7-5175879C7ACC',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE TABLE #RESULT(date smalldatetime,
					preference_type_id int,
					preference_type_name nvarchar(50),
					preference_id int,
					preference_name nvarchar(100),
					preferences_requested int,
					preferences_fulfilled int,
					fulfillment decimal(18,3),
					preferences_unfulfilled int,
					hide_time_zone bit, 
					must_haves int)

INSERT INTO #result(date,preference_type_id,preference_type_name,preference_id,preference_name, preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled,hide_time_zone,must_haves)
SELECT	'2013-09-02',
		1,
		'preference_type_name',
		23,
		'preference_name',
		11,
		17,
		0.5,
		3,
		1,
		8

INSERT INTO #result(date,preference_type_id,preference_type_name,preference_id,preference_name, preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled,hide_time_zone,must_haves)
SELECT	'2013-09-03',
		1,
		'preference_type_name',
		23,
		'preference_name',
		11,
		17,
		0.5,
		3,
		1,
		8

SELECT *  FROM #result


END