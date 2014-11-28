IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_reports_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_reports_load]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2009-01-20
-- Description:	Used by Freemimum to get an empty list fo reports. E.g fake the Analytics reports
-- Change:		Henry Greijer 2010-03-05 Added TargetFrame
-- =============================================

CREATE PROCEDURE [mart].[raptor_reports_load]
AS

CREATE TABLE #report(
	[report_id] [uniqueidentifier] NOT NULL,
	[url] [nvarchar](500) NULL,
	[target] [nvarchar](50) NULL,
	[report_name_resource_key] [nvarchar](50) NOT NULL,
	[Version]  varchar(3) NOT NULL
)

SELECT	report_id						as ReportId,
		'xx' + report_name_resource_key as ReportName, 
                                    url as ReportUrl,
                                    target as TargetFrame,
		[Version]							as 'Version'
FROM #report
