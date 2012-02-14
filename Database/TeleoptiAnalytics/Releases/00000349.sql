----------------  
--Name: Jonas n
--Date: 2012-01-03  
--Desc: Change of report selection dependencies
----------------
UPDATE mart.report_control_collection SET depend_of4 = depend_of3, depend_of3 = 253 WHERE control_collection_id = 257
UPDATE mart.report_control_collection SET depend_of4 = depend_of3, depend_of3 = 264 WHERE control_collection_id = 268
UPDATE mart.report_control_collection SET depend_of4 = depend_of3, depend_of3 = 280 WHERE control_collection_id = 284
UPDATE mart.report_control_collection SET depend_of4 = depend_of3, depend_of3 = 290 WHERE control_collection_id = 294
UPDATE mart.report_control_collection SET depend_of4 = depend_of3, depend_of3 = 302 WHERE control_collection_id = 306
UPDATE mart.report_control_collection SET depend_of4 = depend_of3, depend_of3 = 316 WHERE control_collection_id = 320
UPDATE mart.report_control_collection SET depend_of4 = depend_of3, depend_of3 = 327 WHERE control_collection_id = 331
UPDATE mart.report_control_collection SET depend_of3 = depend_of2, depend_of2 = 339 WHERE control_collection_id = 342
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (349,'7.1.349') 
