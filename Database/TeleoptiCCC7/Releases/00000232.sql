/* 
Trunk initiated: 
2010-04-21 
10:51
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Henry and Robin
--Date: 2010-04-21
--Desc: Added column for absence request processor (= what should happen after passed validation)
----------------  
ALTER TABLE AbsenceRequestOpenPeriod ADD [AbsenceRequestProcess] [int] NOT NULL
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (232,'7.1.232') 
