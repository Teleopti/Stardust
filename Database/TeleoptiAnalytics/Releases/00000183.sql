/* 
Trunk initiated: 
2009-12-09 
15:16
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2009-12-09
--Desc: Default data missing in table
----------------  
--Safty
DELETE FROM [mart].[fact_schedule_preference]
DELETE FROM [mart].[dim_preference_type]

--Insert default data
INSERT INTO [mart].[dim_preference_type]
           (
			   [preference_type_id],
			   [preference_type_name],
			   [resource_key]
           )
     VALUES
           (
				1,
				'Shift Category',
				'ResShiftCategory'
			)

INSERT INTO [mart].[dim_preference_type]
           (
			   [preference_type_id],
			   [preference_type_name],
			   [resource_key]
           )
     VALUES
           (
				2,
				'Day Off',
				'ResDayOff'
			)
INSERT INTO [mart].[dim_preference_type]
           (
			   [preference_type_id],
			   [preference_type_name],
			   [resource_key]
           )
     VALUES
           (
				3,
				'Extended',
				'ResExtendedPreference'
			)

GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (183,'7.0.183') 
