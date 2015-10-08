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
   @date smalldatetime,
   @pageIds nvarchar(max)
AS
BEGIN
   CREATE TABLE #AllPageId (
      Id uniqueidentifier
   );

   INSERT INTO #AllPageId
   SELECT * FROM SplitStringString(@pageIds)

   SELECT DISTINCT PageId
      ,GroupId
      ,GroupName      
      ,CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER) PersonId
      ,'' FirstName
      ,'' LastName
      ,'' EmploymentNumber
      ,CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER) TeamId
      ,CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER) SiteId
      ,BusinessUnitId
   FROM ReadModel.groupingreadonly
   Join #AllPageId on ReadModel.groupingreadonly.PageId = #AllPageId.Id
   WHERE businessunitid = @businessUnitId
      AND @date BETWEEN StartDate
        AND isnull(EndDate, '2059-12-31')
      AND (
        LeavingDate >= @date
        OR LeavingDate IS NULL
        )
   ORDER BY groupname
END
