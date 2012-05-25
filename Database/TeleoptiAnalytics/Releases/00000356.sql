update mart.report_control_collection
set print_order = 9
WHERE control_collection_id = 377

update mart.report_control_collection
set print_order = 10
WHERE control_collection_id = 378

update mart.report_control_collection
set print_order = 11
WHERE control_collection_id = 379

GO


PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (356,'7.1.356') 
