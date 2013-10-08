/****** Object:  StoredProcedure [dbo].[ActivityAlarmMapping]    Script Date: 10/03/2013 15:37:07 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityAlarmMapping]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ActivityAlarmMapping]
GO

CREATE PROCEDURE [dbo].[ActivityAlarmMapping]
-- =============================================
-- Author:		Erik Sundberg
-- Create date: 2013-10-03
-- Description:	Returns list of complete StateGroupActivityAlarm-mappings
-- =============================================
-- Date			Who	Description
-- =============================================
-- exec [dbo].[ActivityAlarmMapping]
AS
SELECT sg.Id StateGroupId , sg.Name StateGroupName, Activity ActivityId, t.Name, t.Id AlarmTypeId,
	t.DisplayColor,t.StaffingEffect, t.ThresholdTime, a.BusinessUnit
	FROM StateGroupActivityAlarm a
	INNER JOIN AlarmType t ON a.AlarmType = t.Id
	LEFT JOIN RtaStateGroup sg ON  a.StateGroup = sg.Id 
	WHERE t.IsDeleted = 0
	AND sg.IsDeleted = 0
UNION ALL								
SELECT NULL,NULL,Activity ActivityId, t.Name, t.Id AlarmTypeId,
	t.DisplayColor,t.StaffingEffect, t.ThresholdTime, a.BusinessUnit
	FROM StateGroupActivityAlarm a
	INNER JOIN AlarmType t ON a.AlarmType = t.Id 
	WHERE t.IsDeleted = 0
	AND a.StateGroup IS NULL
UNION ALL
SELECT sg.Id StateGroupId, sg.Name StateGroupName, Activity ActivityId, NULL, NULL,
	NULL, NULL, NULL, a.BusinessUnit
	FROM StateGroupActivityAlarm a
	LEFT JOIN RtaStateGroup sg on a.StateGroup = sg.Id
	WHERE sg.IsDeleted = 0
	AND a.AlarmType IS NULL
UNION ALL
SELECT NULL, NULL, Activity ActivityId, NULL, NULL,
	NULL, NULL, NULL, a.BusinessUnit
	FROM StateGroupActivityAlarm a
	WHERE a.StateGroup IS NULL
	AND a.AlarmType IS NULL
GO