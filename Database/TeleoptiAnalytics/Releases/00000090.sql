/* 
BuildTime is: 
2009-04-03 
14:20
*/ 
/* 
Trunk initiated: 
2009-04-01 
16:33
By: TOPTINET\johanr 
On INTEGRATION 
*/ 
----------------  
--Name: David Jonsson
--Date: 2009-04-02
--Desc: Patch for deployed environments
----------------
--Declare
DECLARE @time_zone_id int
SET @time_zone_id = NULL

--Init
SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone
WHERE time_zone_code = 'UTC'

--Update
IF (SELECT @time_zone_id) IS NOT NULL
BEGIN
	UPDATE mart.sys_datasource
	SET time_zone_id = @time_zone_id
	WHERE datasource_id = 1 --Hardcode value for "Raptor Default"
END
 
GO 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (90,'7.0.90') 
