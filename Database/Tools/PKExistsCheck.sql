SELECT 
	SCHEMA_NAME(schema_id) + '.' + t.name + ' lacks a PRIMARY KEY' as 'resultline'
FROM sys.tables t
WHERE OBJECTPROPERTY(OBJECT_ID,'TableHasPrimaryKey') = 0
ORDER BY resultline
