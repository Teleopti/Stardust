IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadTeamSchedulesWithFilters]') AND type in (N'P', N'PC'))
DROP PROCEDURE  [ReadModel].[LoadTeamSchedulesWithFilters]
GO