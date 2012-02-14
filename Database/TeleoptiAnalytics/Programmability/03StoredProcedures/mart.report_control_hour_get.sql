IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_hour_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_hour_get]
GO

/*
Last Updated:2008-09-10
20080910 Added parameter @bu_id KJ
20090211 New mart schema
*/
CREATE Proc [mart].[report_control_hour_get]

@report_id int,
@person_code uniqueidentifier,
@language_id int,
@bu_id uniqueidentifier
as



SELECT DISTINCT
	id		= convert(int,left(hour_name,2)),
	name	= hour_name
FROM
	mart.dim_interval

GO

