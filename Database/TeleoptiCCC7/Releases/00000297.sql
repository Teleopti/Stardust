/* 
Trunk initiated: 
2010-10-04 
08:50
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2010-xx-xx  
--Desc: Because ...  
----------------  
 
GO 
 ----------------  
--Name: Peter & david
--Date: 2010-10-08
--Desc: UserDetail is missing
----------------  

INSERT INTO [dbo].[UserDetail]
SELECT
           [Id]                             = newid(),
           [LastPasswordChange]             = dateadd(day,-1,getdate()),
           [InvalidAttemptsSequenceStart]   = dateadd(day,-1,getdate()),
           [IsLocked]                       = 0,
           [InvalidAttempts]                = 0,
           [Person]                         = p.id
FROM dbo.person p
WHERE NOT EXISTS (SELECT 1 FROM dbo.UserDetail u WHERE u.person = p.Id)


GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (297,'7.1.297') 
