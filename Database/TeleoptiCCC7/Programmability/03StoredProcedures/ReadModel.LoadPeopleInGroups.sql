IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPeopleInGroups]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPeopleInGroups]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Chundan Xu
-- Create date: 2017-08-10
-- Description: load people in given groups
-- =============================================
CREATE PROCEDURE [ReadModel].[LoadPeopleInGroups]
   @businessUnitId uniqueidentifier,
   @startDate smalldatetime,
   @endDate smalldatetime,
   @groupIds nvarchar(max)
AS
BEGIN
   CREATE TABLE #AllGroupId (
      Id uniqueidentifier
   );

   INSERT INTO #AllGroupId
   SELECT * FROM SplitStringString(@groupIds)

   SELECT PersonId, FirstName, LastName, EmploymentNumber, TeamId, SiteId, BusinessUnitId
   FROM ReadModel.groupingreadonly
   Join #AllGroupId on ReadModel.groupingreadonly.GroupId = #AllGroupId.Id
   WHERE businessunitid = @businessUnitId
      AND @startDate <= isnull(EndDate, '2059-12-31')
	  AND @endDate >= isnull(StartDate, '1900-01-01')
      AND (
        LeavingDate >= @startDate
        OR LeavingDate IS NULL
        )
   ORDER BY groupname
END
