/* 
Trunk initiated: 
2011-05-12 
15:44
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Ola 
--Date: 2011-05-16  
--Desc: Because when exporting meeting we want to keep track of the original meeting 
----------------  
ALTER TABLE Meeting Add
OriginalMeetingId uniqueidentifier
GO
ALTER TABLE AuditTrail.Meeting Add
OriginalMeetingId uniqueidentifier



----------------  
--Name: Roger
--Date: 2011-05-17  
--Desc: Cascading deletes on person assignment aggregate
 
 -------------------
--MainShift
-------------------
ALTER TABLE dbo.MainShift
           DROP CONSTRAINT FK_MainShift_PersonAssignment

ALTER TABLE dbo.MainShiftActivityLayer
           DROP CONSTRAINT FK_MainShiftActivityLayer_MainShift

ALTER TABLE dbo.MainShift ADD CONSTRAINT
           FK_MainShift_PersonAssignment FOREIGN KEY
           (
           Id
           ) REFERENCES dbo.PersonAssignment
           (
           Id
           ) 
            ON DELETE  CASCADE 

ALTER TABLE dbo.MainShiftActivityLayer ADD CONSTRAINT
           FK_MainShiftActivityLayer_MainShift FOREIGN KEY
           (
           Parent
           ) REFERENCES dbo.MainShift
           (
           Id
           ) 
            ON DELETE CASCADE

---------------------
--Overtimeshift+Personal
--------------------
ALTER TABLE dbo.OvertimeShift
           DROP CONSTRAINT FK_OvertimeShift_PersonAssignment

ALTER TABLE dbo.PersonalShift
           DROP CONSTRAINT FK_PersonalShift_PersonAssignment


ALTER TABLE dbo.OvertimeShiftActivityLayer
           DROP CONSTRAINT FK_OvertimeShiftActivityLayer_OvertimeShift

ALTER TABLE dbo.OvertimeShift ADD CONSTRAINT
           FK_OvertimeShift_PersonAssignment FOREIGN KEY
           (
           Parent
           ) REFERENCES dbo.PersonAssignment
           (
           Id
           ) 
            ON DELETE  CASCADE 

ALTER TABLE dbo.OvertimeShiftActivityLayer ADD CONSTRAINT
           FK_OvertimeShiftActivityLayer_OvertimeShift FOREIGN KEY
           (
           Parent
           ) REFERENCES dbo.OvertimeShift
           (
           Id
           ) 
            ON DELETE  CASCADE 

ALTER TABLE dbo.PersonalShiftActivityLayer
           DROP CONSTRAINT FK_PersonalShiftActivityLayer_PersonalShift

ALTER TABLE dbo.PersonalShift ADD CONSTRAINT
           FK_PersonalShift_PersonAssignment FOREIGN KEY
           (
           Parent
           ) REFERENCES dbo.PersonAssignment
           (
           Id
           ) 
            ON DELETE  CASCADE 

ALTER TABLE dbo.PersonalShiftActivityLayer ADD CONSTRAINT
           FK_PersonalShiftActivityLayer_PersonalShift FOREIGN KEY
           (
           Parent
           ) REFERENCES dbo.PersonalShift
           (
           Id
           ) 
            ON DELETE  CASCADE 

----------------  
--Name: AndersF
--Date: 2011-05-23  
--Desc: Fix bad default dates for StudAvail 
----------------  
UPDATE dbo.WorkflowControlSet SET
	StudentAvailabilityPeriodFromDate = '2011-03-01T00:00:00',
	StudentAvailabilityPeriodToDate = '2011-03-31T00:00:00',
	StudentAvailabilityInputFromDate = '2011-01-01T00:00:00',
	StudentAvailabilityInputToDate = '2011-01-31T00:00:00'
WHERE
StudentAvailabilityPeriodFromDate = '1901-01-01T00:00:00' and
	StudentAvailabilityPeriodToDate = '2029-01-01T00:00:00' and
	StudentAvailabilityInputFromDate = '1901-01-01T00:00:00' and
	StudentAvailabilityInputToDate = '2029-01-01T00:00:00'
 
GO 
 


PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (328,'7.1.328') 
