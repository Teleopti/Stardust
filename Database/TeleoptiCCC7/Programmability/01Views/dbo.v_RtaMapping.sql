IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_RtaMapping]'))
DROP VIEW [dbo].[v_RtaMapping]
GO

CREATE VIEW [dbo].[v_RtaMapping]
WITH SCHEMABINDING
AS

SELECT
	ISNULL(m.BusinessUnit, ag.StateGroupBU) AS BusinessUnitId,

	s.StateCode AS StateCode,
	s.PlatformTypeId AS PlatformTypeId,
	ag.StateGroupId,
	ag.StateGroupName,

	ag.ActivityId,

	r.Id AS RuleId,
	r.Name AS RuleName,
	r.Adherence AS AdherenceInt,
	r.StaffingEffect AS StaffingEffect,
	r.DisplayColor AS DisplayColor,

	r.IsAlarm AS IsAlarm,
	r.AlarmColor AS AlarmColor,
	r.ThresholdTime AS ThresholdTime

FROM
(
	SELECT 
		a.Id ActivityId, 
		g.Id StateGroupId, 
		g.Name StateGroupName, 
		g.BusinessUnit StateGroupBU 
	FROM
		(
			SELECT Id FROM dbo.Activity 
			UNION 
			SELECT NULL
		) a,
		(
			SELECT Id, Name, BusinessUnit FROM dbo.RtaStateGroup 
			UNION 
			SELECT NULL, NULL, NULL
		) g
) ag
LEFT JOIN dbo.RtaMap m ON
	ISNULL(m.Activity, '00000000-0000-0000-0000-000000000000') = ISNULL(ag.ActivityId, '00000000-0000-0000-0000-000000000000') AND
	ISNULL(m.StateGroup, '00000000-0000-0000-0000-000000000000') = ISNULL(ag.StateGroupId, '00000000-0000-0000-0000-000000000000')
LEFT JOIN dbo.RtaRule r ON
	r.Id = m.RtaRule
LEFT JOIN dbo.RtaState s ON
	s.Parent = ag.StateGroupId


GO
