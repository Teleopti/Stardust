IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_schedulingtype_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_schedulingtype_get]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2013-09-30
-- Description:	Loads Scheduling option
-- =============================================
CREATE PROCEDURE [mart].[report_control_schedulingtype_get] 
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS

--exec mart.report_control_schedulingtype_get @report_id='F7F3AF97-EC24-4EA8-A2C7-5175879C7ACC',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@language_id=1053,@bu_id='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE TABLE #schedulingtype (counter int identity(1,1),id int, name nvarchar(50)) 

INSERT INTO #schedulingtype 
SELECT 1,'Both'
UNION ALL
SELECT 2,'Scheduling'
UNION ALL
SELECT 3,'Optimization'

select Id,Name FROM #schedulingtype 

GO

