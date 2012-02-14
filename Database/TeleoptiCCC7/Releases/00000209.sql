/* 
Trunk initiated: 
2010-03-02 
16:32
By: TOPTINET\andersf 
On ANDERSFNC8430 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-03-03
--Desc: Delete PersonalShift, OvertimeShift and MainShift without childs and then fix OrderIndex in flux
--		Finally, check if there is any PersonAssignment without childs (main OR overtime OR Overtime)
----------------  
--delete personalshift without childs
PRINT 'Delete Shifts without shiftactivitylayer. Working ...'

PRINT '	1) Personalshift. Working ...'
DELETE FROM personalshift
WHERE Id IN
(
SELECT id FROM personalshift ps
WHERE NOT EXISTS(SELECT 1 FROM personalshiftactivitylayer psa WHERE ps.Id=psa.Parent)
)

--update orderindex in flux (after potetial delete)
UPDATE personalshift
SET 
	OrderIndex	= sub.NewOrderindex
FROM
( 
	SELECT
		ps1.Id,
		ps1.Parent,
		ps1.OrderIndex,
		personalshiftCheck.rn-1 as NewOrderindex
	FROM personalshift ps1
	INNER JOIN
	(
		SELECT ps2.id, ps2.parent, ps2.orderindex, ROW_NUMBER()OVER(PARTITION BY ps2.parent ORDER BY ps2.orderindex) rn
		FROM personalshift ps2
	) personalshiftCheck
	ON ps1.id=personalshiftCheck.id
	WHERE personalshiftCheck.OrderIndex <> personalshiftCheck.rn-1
)sub
WHERE sub.Id = personalshift.Id AND sub.Parent = personalshift.Parent
PRINT '	1) Personalshift. Done!'

--delete Overtimeshift without childs
PRINT '	2) Overtimeshift. Working ...'
DELETE FROM Overtimeshift
WHERE Id IN
(
SELECT id FROM Overtimeshift ps
WHERE NOT EXISTS(SELECT 1 FROM Overtimeshiftactivitylayer psa WHERE ps.Id=psa.Parent)
)

--update orderindex in flux (after potetial delete)
UPDATE Overtimeshift
SET 
	OrderIndex	= sub.NewOrderindex
FROM
( 
	SELECT
		ps1.Id,
		ps1.Parent,
		ps1.OrderIndex,
		OvertimeshiftCheck.rn-1 as NewOrderindex
	FROM Overtimeshift ps1
	INNER JOIN
	(
		SELECT ps2.id, ps2.parent, ps2.orderindex, ROW_NUMBER()OVER(PARTITION BY ps2.parent ORDER BY ps2.orderindex) rn
		FROM Overtimeshift ps2
	) OvertimeshiftCheck
	ON ps1.id=OvertimeshiftCheck.id
	WHERE OvertimeshiftCheck.OrderIndex <> OvertimeshiftCheck.rn-1
)sub
WHERE sub.Id = Overtimeshift.Id AND sub.Parent = Overtimeshift.Parent
PRINT '	2) Overtimeshift. Done!'

--delete Mainshift without childs
PRINT '	3) Mainshift. Working ...'
DELETE FROM Mainshift
WHERE Id IN
(
SELECT id FROM Mainshift ps
WHERE NOT EXISTS(SELECT 1 FROM Mainshiftactivitylayer psa WHERE ps.Id=psa.Parent)
)
PRINT '	3) Mainshift. Done!'

--delete any PersonAssignment without any Childs (at all)
PRINT '	4) PersonAssignment. Working ...'
DELETE FROM PersonAssignment
WHERE Id IN
(
	SELECT Id FROM PersonAssignment pa WHERE NOT EXISTS
	(
		SELECT 1 FROM
		(
			SELECT Id FROM Mainshift
			UNION ALL
			SELECT Parent as Id FROM Overtimeshift
			UNION ALL
			SELECT Parent as Id FROM personalshift
		) Childs
		WHERE Childs.Id=pa.id
	)
)
PRINT '	4) PersonAssignment. Done!'
PRINT 'Delete Shifts without shiftactivitylayer. Done!'
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (209,'7.1.209') 
