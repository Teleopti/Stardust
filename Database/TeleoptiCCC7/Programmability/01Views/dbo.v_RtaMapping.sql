IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_RtaMapping]'))
DROP VIEW [dbo].[v_RtaMapping]
GO

CREATE VIEW [dbo].[v_RtaMapping]
WITH SCHEMABINDING
AS

SELECT
	ISNULL(m.BusinessUnit, g.BusinessUnit) AS BusinessUnitId,

	s.StateCode AS StateCode,
	s.PlatformTypeId AS PlatformTypeId,
	g.Id AS StateGroupId,
	g.Name AS StateGroupName,
	g.IsLogOutState AS IsLogOutState,

	m.Activity AS ActivityId,

	r.Id AS RuleId,
	r.Name AS RuleName,
	r.Adherence AS AdherenceInt,
	r.StaffingEffect AS StaffingEffect,
	r.DisplayColor AS DisplayColor,

	r.IsAlarm AS IsAlarm,
	r.AlarmColor AS AlarmColor,
	r.ThresholdTime AS ThresholdTime

FROM 
	dbo.RtaMap m
FULL JOIN dbo.RtaStateGroup g ON
	g.Id = m.StateGroup
FULL JOIN dbo.RtaState s ON
	s.Parent = g.Id
LEFT JOIN dbo.RtaRule r ON
	r.Id = m.RtaRule

GO
