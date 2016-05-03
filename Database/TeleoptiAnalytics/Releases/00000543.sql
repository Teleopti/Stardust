CREATE TABLE #rcc(collection_id int,CollectionId uniqueidentifier) 
INSERT #rcc(collection_id,CollectionId)
SELECT collection_id,CollectionId 
from mart.report_control_collection where collection_id in (25,28,29,33,35,36,39,40,41) 
AND print_order=2
order by collection_id

UPDATE mart.report_control_collection 
SET control_id=44
WHERE Id='53B2673C-9057-4119-9EF8-4FD2CF46E6D7'

UPDATE mart.report_control_collection 
SET control_id=44
WHERE Id='192E6E3D-59F3-4DA8-8BB5-DB16CC0C64F0'

UPDATE mart.report_control_collection
SET CollectionId = #rcc.CollectionId
FROM #rcc 
INNER JOIN mart.report_control_collection ON #rcc.collection_id=mart.report_control_collection.collection_id
WHERE mart.report_control_collection.control_id=44
