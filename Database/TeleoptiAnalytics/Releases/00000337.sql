
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (337,'7.1.337') 

--Anders F: Blanks prevents updates in RTA
update mart.dim_acd_login set acd_login_original_id = LTRIM(RTRIM(acd_login_original_id))
