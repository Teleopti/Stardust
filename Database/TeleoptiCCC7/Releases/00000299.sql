/* 
Trunk initiated: 
2010-10-04 
10:50
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-10-20
--Desc: Adding Minimum+Maximum to dbo.PersonAssignment + AuditTrail.PersonAssignment
----------------  

--DROP TRIGGERS, they will be re-created later (programability)
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_OvertimeShift_I]'))
	DROP TRIGGER [dbo].[tr_OvertimeShift_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_OvertimeShift_UD]'))
	DROP TRIGGER [dbo].[tr_OvertimeShift_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_OvertimeShiftActivityLayer_I]'))
	DROP TRIGGER [dbo].[tr_OvertimeShiftActivityLayer_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_OvertimeShiftActivityLayer_UD]'))
	DROP TRIGGER [dbo].[tr_OvertimeShiftActivityLayer_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_MainShiftActivityLayer_I]'))
	DROP TRIGGER [dbo].[tr_MainShiftActivityLayer_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_MainShiftActivityLayer_UD]'))
	DROP TRIGGER [dbo].[tr_MainShiftActivityLayer_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_MainShift_I]'))
	DROP TRIGGER [dbo].[tr_MainShift_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_MainShift_UD]'))
	DROP TRIGGER [dbo].[tr_MainShift_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonAssignment_I]'))
	DROP TRIGGER [dbo].[tr_PersonAssignment_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonAssignment_UD]'))
	DROP TRIGGER [dbo].[tr_PersonAssignment_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonalShiftActivityLayer_I]'))
	DROP TRIGGER [dbo].[tr_PersonalShiftActivityLayer_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonalShiftActivityLayer_UD]'))
	DROP TRIGGER [dbo].[tr_PersonalShiftActivityLayer_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonalShift_I]'))
	DROP TRIGGER [dbo].[tr_PersonalShift_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonalShift_UD]'))
	DROP TRIGGER [dbo].[tr_PersonalShift_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonAbsence_I]'))
	DROP TRIGGER [dbo].[tr_PersonAbsence_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonAbsence_UD]'))
	DROP TRIGGER [dbo].[tr_PersonAbsence_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonDayOff_I]'))
	DROP TRIGGER [dbo].[tr_PersonDayOff_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_PersonDayOff_UD]'))
	DROP TRIGGER [dbo].[tr_PersonDayOff_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_Meeting_I]'))
	DROP TRIGGER [dbo].[tr_Meeting_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_Meeting_UD]'))
	DROP TRIGGER [dbo].[tr_Meeting_UD]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_MeetingPerson_I]'))
	DROP TRIGGER [dbo].[tr_MeetingPerson_I]
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[tr_MeetingPerson_UD]'))
	DROP TRIGGER [dbo].[tr_MeetingPerson_UD]
GO

--ALTER PersonAssignment ADD 2 NEW COLUMNS 
ALTER TABLE dbo.PersonAssignment ADD
	Minimum datetime NULL,
	Maximum datetime NULL
GO

--Take care of the Shadow table ADD 2 NEW COLUMNS
ALTER TABLE AuditTrail.PersonAssignment ADD
	Minimum datetime NULL,
	Maximum datetime NULL

SELECT PersonAssignment.Id,MainShiftActivityLayer.Parent,MIN(MainShiftActivityLayer.Minimum)'minimum',MAX(MainShiftActivityLayer.Maximum)'maximum'
INTO #MainShiftActivityLayer
FROM PersonAssignment
INNER JOIN MainShiftActivityLayer ON PersonAssignment.Id=MainShiftActivityLayer.Parent
GROUP BY PersonAssignment.Id,MainShiftActivityLayer.Parent

--select * from #MainShiftActivityLayer

--MainShiftActivityLayer
UPDATE PersonAssignment
SET Minimum=m.minimum,	Maximum=m.Maximum 
FROM #MainShiftActivityLayer m
WHERE PersonAssignment.Id=m.Id

--SELECT * FROM PersonAssignment

SELECT PersonAssignment.Id,PersonalShift.Parent'PersonalShift_Parent',PersonalShiftActivityLayer.Parent 'PersonalShiftActivityLayer_Parent',
	MIN(PersonalShiftActivityLayer.Minimum)'minimum',MAX(PersonalShiftActivityLayer.Maximum)'maximum'
INTO #PersonalShiftActivityLayer
FROM PersonAssignment
INNER JOIN PersonalShift ON PersonalShift.Parent=PersonAssignment.Id
INNER JOIN PersonalShiftActivityLayer ON PersonalShiftActivityLayer.Parent=PersonalShift.Id
GROUP BY PersonAssignment.Id,PersonalShift.Parent,PersonalShiftActivityLayer.Parent


UPDATE PersonAssignment
SET Minimum=p.minimum
FROM #PersonalShiftActivityLayer p
WHERE PersonAssignment.Id=p.Id AND (p.Minimum < PersonAssignment.Minimum OR PersonAssignment.Minimum IS NULL)

UPDATE PersonAssignment
SET Maximum=p.Maximum 
FROM #PersonalShiftActivityLayer p
WHERE PersonAssignment.Id=p.Id AND (p.Maximum > PersonAssignment.Maximum OR PersonAssignment.Maximum IS NULL)

--select * from PersonAssignment

ALTER INDEX ALL ON dbo.PersonAssignment REBUILD WITH (FILLFACTOR = 90)

--OvertimeShiftActivityLayer
SELECT PersonAssignment.Id,OvertimeShift.Parent'OvertimeShift_Parent',OvertimeShiftActivityLayer.Parent 'OvertimeShiftActivityLayer_Parent',
	MIN(OvertimeShiftActivityLayer.Minimum)'minimum',MAX(OvertimeShiftActivityLayer.Maximum)'maximum'
INTO #OvertimeShiftActivityLayer
FROM PersonAssignment
INNER JOIN OvertimeShift ON OvertimeShift.Parent=PersonAssignment.Id
INNER JOIN OvertimeShiftActivityLayer ON OvertimeShiftActivityLayer.Parent=OvertimeShift.Id
GROUP BY PersonAssignment.Id,OvertimeShift.Parent,OvertimeShiftActivityLayer.Parent

UPDATE PersonAssignment
SET Minimum=o.minimum
FROM #OvertimeShiftActivityLayer o
WHERE PersonAssignment.Id=o.Id AND (o.Minimum < PersonAssignment.Minimum OR PersonAssignment.Minimum IS NULL)

UPDATE PersonAssignment
SET Maximum=o.Maximum 
FROM #OvertimeShiftActivityLayer o
WHERE PersonAssignment.Id=o.Id AND (o.Maximum > PersonAssignment.Maximum OR PersonAssignment.Maximum IS NULL)

GO
DROP TABLE #OvertimeShiftActivityLayer,#PersonalShiftActivityLayer,#MainShiftActivityLayer
GO

--Clean up NULL data (unwanted data)
delete dbo.MainShift
from dbo.MainShift ms
inner join dbo.personassignment pa on ms.id = pa.id
where pa.minimum is null or pa.maximum is null

delete from dbo.PersonAssignment where minimum is null or maximum is null

--Set not null
ALTER TABLE dbo.PersonAssignment ALTER COLUMN
           Minimum datetime NOT NULL
ALTER TABLE dbo.PersonAssignment ALTER COLUMN         
           Maximum datetime NOT NULL

--Set not null
ALTER TABLE dbo.PersonAssignment ALTER COLUMN
	Minimum datetime NOT NULL
ALTER TABLE dbo.PersonAssignment ALTER COLUMN	
	Maximum datetime NOT NULL

UPDATE AuditTrail.PersonAssignment
	SET Minimum = '1900-01-01', Maximum	= '2059-12-31'

--Set not null
ALTER TABLE AuditTrail.PersonAssignment ALTER COLUMN
	Minimum datetime NOT NULL
ALTER TABLE AuditTrail.PersonAssignment ALTER COLUMN	
	Maximum datetime NOT NULL

--Add new index to support Min/Max on PersonAssignment
CREATE NONCLUSTERED INDEX IX_Scenario_Minimum_Maximum
ON [dbo].[PersonAssignment] (
	[Scenario],
	[Minimum],
	[Maximum]
	)
INCLUDE ([Id])
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (299,'7.1.299') 
