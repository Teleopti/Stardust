/* 
Trunk initiated: 
2010-12-08 
08:58
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-12-22
--Desc: Clean up obsolete SPs regarding Adherance
--Name: David Jonsson  
--Date: 2011-01-03
--Desc: Rename all PKs  
----------------  
DECLARE @tablename sysname
DECLARE @pkname sysname
DECLARE @newpkname sysname
DECLARE @schema sysname
DECLARE @DynamicSQL nvarchar(4000)

DECLARE cur CURSOR FOR
SELECT 
s2.name AS tablename, 
s1.name AS PKname,
s3.name
FROM sys.objects s1
INNER JOIN sys.objects s2 ON s2.object_id = s1.parent_object_id
inner join sys.schemas s3
on s1.schema_id = s3.schema_id
WHERE OBJECTPROPERTY(s1.object_id, N'IsPrimaryKey') = 1
AND s1.name <> 'PK_'+s2.name
ORDER BY s1.name;
OPEN cur;
	FETCH NEXT FROM cur INTO @tablename, @pkname, @schema;
	WHILE @@FETCH_STATUS = 0
	BEGIN
	SET @newpkname = 'PK_' + @tablename
	SELECT @DynamicSQL = 'sp_rename N''[' + @schema + '].[' + @tablename + '].['+ @pkname +']'', N'''+ @newpkname +''', N''INDEX'''

	PRINT @pkname + ' > ' + @newpkname
	PRINT @DynamicSQL 
	EXEC sp_executesql @DynamicSQL 


FETCH NEXT FROM cur INTO @tablename, @pkname, @schema;
END
CLOSE cur;
DEALLOCATE cur;
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_myreport_load_agent_adherence_info]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_myreport_load_agent_adherence_info]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_myreport_load_agent_detail_adherence_info]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_myreport_load_agent_detail_adherence_info]
GO
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (310,'7.1.310') 
