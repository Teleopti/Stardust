/* 
Trunk initiated: 
2011-05-30 
10:16
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: AndersF  
--Date: 2011-06-01  
--Desc: It should be 25
----------------  
update RtaState
set StateCode = LEFT(StateCode,25)

ALTER TABLE [dbo].[RtaState] ALTER COLUMN [StateCode] nvarchar(25)

GO
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (329,'7.1.329') 
