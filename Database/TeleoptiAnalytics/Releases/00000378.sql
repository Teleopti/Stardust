----------------  
--Name: Karin and David
--Date: 2012-12-12
--Desc: PBI21517
----------------  
UPDATE mart.report
SET help_key = REPLACE(help_key,'.html','')
WHERE RIGHT(help_key,5) = '.html'

UPDATE mart.report
SET help_key = REPLACE(help_key,'f01_','f01:')
WHERE LEFT(help_key,4) = 'f01_'

UPDATE mart.report
SET help_key = REPLACE(help_key,'_','+')
WHERE SUBSTRING(help_key,11,1) = '_'

UPDATE mart.report
SET help_key='f01:Report+AbsenceTimePerAbsence'
WHERE Id='D45A8874-57E1-4EB9-826D-E216A4CBC45B'

UPDATE mart.report
SET help_key='f01:Report+AbsenceTimePerAgent'
WHERE Id='C5B88862-F7BE-431B-A63F-3DD5FF8ACE54'
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (378,'7.3.378') 
