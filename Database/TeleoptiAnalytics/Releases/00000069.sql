/* 
BuildTime is: 
2009-02-16 
18:31
*/ 
/* 
Trunk initiated: 
2009-02-16 
09:06
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  

----------------  
--Name: KJ
--Date: 2009-02-16
--Desc: Missing table data for preferences  
----------------  
SET IDENTITY_INSERT [mart].[dim_preference_type] ON
INSERT [mart].[dim_preference_type] ([preference_type_id], [preference_type_name], [term_id]) VALUES (1, N'Shift Category', 29)
INSERT [mart].[dim_preference_type] ([preference_type_id], [preference_type_name], [term_id]) VALUES (2, N'Day Off', 28)
INSERT [mart].[dim_preference_type] ([preference_type_id], [preference_type_name], [term_id]) VALUES (3, N'Extended Preference', 30)
SET IDENTITY_INSERT [mart].[dim_preference_type] OFF
GO
 
GO 
 

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (69,'7.0.69') 
