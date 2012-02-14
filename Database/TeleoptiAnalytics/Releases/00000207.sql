/* 
Trunk initiated: 
2010-02-09 
08:42
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-02-25 
--Desc: If customer executed Datamart delete SP we are missing data!
----------------  
SET NOCOUNT ON

--update if exists; insert if missing
	IF EXISTS (SELECT 1 FROM mart.dim_preference_type WHERE preference_type_id = 1)
	UPDATE mart.dim_preference_type SET preference_type_name='Shift Category',resource_key='ResShiftCategory' WHERE preference_type_id = 1
	ELSE
	INSERT INTO mart.dim_preference_type(preference_type_id,preference_type_name,resource_key) VALUES (1,'Shift Category','ResShiftCategory')
	
	IF EXISTS (SELECT 1 FROM mart.dim_preference_type WHERE preference_type_id = 2)
	UPDATE mart.dim_preference_type SET preference_type_name='Day Off',resource_key='ResDayOff' WHERE preference_type_id = 2
	ELSE
	INSERT INTO mart.dim_preference_type(preference_type_id,preference_type_name,resource_key) VALUES (2,'Day Off','ResDayOff')
	
	IF EXISTS (SELECT 1 FROM mart.dim_preference_type WHERE preference_type_id = 3)
	UPDATE mart.dim_preference_type SET preference_type_name='Extended',resource_key='ResExtendedPreference' WHERE preference_type_id = 3
	ELSE
	INSERT INTO mart.dim_preference_type(preference_type_id,preference_type_name,resource_key) VALUES (3,'Extended','ResExtendedPreference')

 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (207,'7.1.207') 
