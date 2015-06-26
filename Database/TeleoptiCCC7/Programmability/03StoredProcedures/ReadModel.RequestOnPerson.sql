
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[RequestOnPerson]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[RequestOnPerson]
GO

-- exec [ReadModel].RequestOnPerson '4A7A2ACE-436A-4A47-96FF-9CC300CE6ACF', 1, 51
-- =============================================
-- Author:		Ola
-- Create date: 2012-03-15
-- Description:	Gets the old requests to show in new window from request view in scheduler
-- Change Log
CREATE PROCEDURE [ReadModel].[RequestOnPerson]
@person uniqueidentifier,
@start_row int,
@end_row int
AS
SET NOCOUNT ON

CREATE TABLE #result(
	[Id] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
------------------------------------------------
-- When			Who		What
-- =============================================
	[EndDateTime] [datetime] NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[EmploymentNumber] [nvarchar](100) NOT NULL,
	[RequestStatus] [int] NOT NULL,
	[Subject] [nvarchar](100) NULL,
	[Message] [nvarchar](2000) NULL,
	[DenyReason] [nvarchar](300) NOT NULL,
	[Info] [nvarchar](100) NOT NULL,
	[RequestType] [varchar](50) NOT NULL,
	[ShiftTradeStatus] [int] NOT NULL,
	[SavedByFirstName] [nvarchar](100) NOT NULL,
	[SavedByLastName] [nvarchar](100) NOT NULL,
	[SavedByEmploymentNumber] [nvarchar](100) NOT NULL,
	[LastUpdatedDateTime] [datetime] NOT NULL
)

INSERT INTO #result
SELECT p.Id, StartDateTime, EndDateTime, p.FirstName, p.LastName, p.EmploymentNumber, RequestStatus, Subject, Message, DenyReason,
a.Name AS Info, 'ABS' as RequestType, 0 as ShiftTradeStatus, 
p2.FirstName as SavedByFirstName  , p2.LastName AS SavedByLastName, p2.EmploymentNumber as SavedByEmploymentNumber, pr.UpdatedOn 
FROM PersonRequest pr INNER JOIN Person p
ON p.Id = pr.Person AND pr.IsDeleted = 0 AND RequestStatus IN(1,2,4)
INNER JOIN Request r ON r.Parent = pr.Id
INNER JOIN Person p2 ON p2.Id = pr.UpdatedBy
INNER JOIN AbsenceRequest ar ON ar.Request = r.Id
INNER JOIN Absence a ON a.Id = ar.Absence
WHERE p.Id = @person

UNION

SELECT p.Id, StartDateTime, EndDateTime, p.FirstName, p.LastName, p.EmploymentNumber, RequestStatus, Subject, Message, DenyReason,
'', 'TEXT' as RequestType, 0 as ShiftTradeStatus, p2.FirstName, p2.LastName, p2.EmploymentNumber , pr.UpdatedOn 
FROM PersonRequest pr INNER JOIN Person p
ON p.Id = pr.Person AND pr.IsDeleted = 0 AND RequestStatus IN(1,2)
INNER JOIN Request r ON r.Parent = pr.Id
INNER JOIN Person p2 ON p2.Id = pr.UpdatedBy
INNER JOIN TextRequest tr ON tr.Request = r.Id
WHERE p.Id = @person

UNION

SELECT p.Id, StartDateTime, EndDateTime, p.FirstName, p.LastName, p.EmploymentNumber, RequestStatus, Subject, Message, DenyReason,
p3.FirstName + ' ' + p3.LastName, 
 'TRADE' as RequestType, ShiftTradeStatus, p2.FirstName, p2.LastName, p2.EmploymentNumber, pr.UpdatedOn 
FROM PersonRequest pr INNER JOIN Person p
ON p.Id = pr.Person AND pr.IsDeleted = 0 AND RequestStatus IN(1,2)
INNER JOIN Request r ON r.Parent = pr.Id
INNER JOIN Person p2 ON p2.Id = pr.UpdatedBy
INNER JOIN ShiftTradeRequest tr ON tr.Request = r.Id
INNER JOIN ShiftTradeSwapDetail d ON tr.Request = d.Parent
INNER JOIN Person p3 ON PersonTo = p3.Id

WHERE p.Id = @person


DECLARE @tot int
SELECT @tot = COUNT(*) FROM #result
--SELECT * FROM #result

SELECT @tot AS TotalCount, *      
FROM (SELECT *, ROW_NUMBER() OVER
(ORDER BY StartDateTime desc) AS RowNumber  
FROM    #result PC) #result WHERE  RowNumber >= @start_row AND RowNumber < @end_row

DROP TABLE #result

