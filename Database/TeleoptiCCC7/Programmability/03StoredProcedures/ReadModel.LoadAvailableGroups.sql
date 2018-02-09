IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadAvailableGroups]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadAvailableGroups]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Xinfeng Li
-- Create date: 2015-10-08
-- Description: To get all available groups by given page ids
-- =============================================
CREATE PROCEDURE [ReadModel].[LoadAvailableGroups]
   @businessUnitId uniqueidentifier,
   @startDate smalldatetime,
   @endDate smalldatetime,
   @pageIds nvarchar(max)
AS
BEGIN
   CREATE TABLE #AllPageId (
      Id uniqueidentifier
   );

   INSERT INTO #AllPageId
   SELECT * FROM SplitStringString(@pageIds)

   SELECT DISTINCT PageId
        , GroupId
        , GroupName
        , CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER) PersonId
        , '' FirstName
        , '' LastName
        , '' EmploymentNumber
        , TeamId
        , SiteId
        , BusinessUnitId
     FROM ReadModel.GroupingReadOnly
     JOIN #AllPageId on ReadModel.GroupingReadOnly.PageId = #AllPageId.Id
    WHERE BusinessUnitId = @businessUnitId
      AND @startDate <= EndDate
      AND @endDate >= StartDate
      AND (LeavingDate >= @startDate OR LeavingDate IS NULL)
    ORDER BY groupname
END
