----------------  
--Name: Kunning  
--Date: 2015-01-26  
--Desc: Remove winform mytime related application functions  
----------------  
UPDATE dbo.ApplicationFunction
SET  IsDeleted = 1
WHERE  (ForeignId = '0019' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0020' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0026' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0027' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0028' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0029' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0030' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0031' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0036' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0051' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0060' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0063' AND ForeignSource = 'Raptor')
	OR (ForeignId = '0069' AND ForeignSource = 'Raptor')
	
GO
