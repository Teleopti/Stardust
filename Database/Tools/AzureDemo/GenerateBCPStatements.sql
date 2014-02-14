/*
Generates BCP statements for export and import of data. Enter corresponding Soure- and Destination credentials and ouput path for the .bat files to be generated.
*/

/*
:SETVAR DESTDB "Forecasts_TeleoptiCCC"
:SETVAR WORKINGDIR "c:\temp\AzureRestore"
:SETVAR SOURCEUSER "bcpUser"
:SETVAR SOURCEPWD "abc123456"
:SETVAR DESTSERVER "tcp:s8v4m110k9.database.windows.net"
:SETVAR DESTUSER "forecast@s8v4m110k9"
:SETVAR DESTPWD "teleopti2012"
*/
SET NOCOUNT ON

-- Source server and database
DECLARE @SourceServer NVARCHAR(100)
DECLARE @SourceDB NVARCHAR(100)
DECLARE @SourceUser NVARCHAR(100)
DECLARE @SourcePwd NVARCHAR(100)
SELECT @SourceServer = @@SERVERNAME
SELECT @SourceDB = DB_NAME()
SELECT @SourceUser = '$(SOURCEUSER)'
SELECT @SourcePwd = '$(SOURCEPWD)'

-- Destination server and database
DECLARE @DestServer NVARCHAR(100)
DECLARE @DestDB NVARCHAR(100)
DECLARE @DestUser NVARCHAR(100)
DECLARE @DestPwd NVARCHAR(100)
SELECT @DestServer = '$(DESTSERVER)'
SELECT @DestDB = '$(DESTDB)'
SELECT @DestUser = '$(DESTUSER)'
SELECT @DestPwd = '$(DESTPWD)'

DECLARE @Path NVARCHAR(1000)
SELECT @Path = '$(WORKINGDIR)' + '\' + @SourceDB

DECLARE @Command NVARCHAR(1200)
SELECT @Command = 'IF NOT EXIST "'+@Path+'" MKDIR "'+@Path+'"'
EXEC xp_cmdshell @Command 

DECLARE @bcpString NVARCHAR(500)

--create the lcoal bcp user
IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = N'$(SOURCEUSER)')
DROP LOGIN [$(SOURCEUSER)]

CREATE LOGIN [$(SOURCEUSER)] WITH PASSWORD=N'$(SOURCEPWD)', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
EXEC sys.sp_addsrvrolemember @loginame = N'$(SOURCEUSER)', @rolename = N'sysadmin'
ALTER LOGIN [$(SOURCEUSER)] ENABLE
----

CREATE TABLE #TableList(SchemaName nvarchar(100), TableName nvarchar(200), Level int, IsLeafLevel int);
with fk_tables as (
     select    s1.schema_id as from_schema_id
     ,        o1.object_id as from_table_id
     ,        s2.schema_id as to_schema_id
     ,        o2.object_id as to_table_id
     from    sys.foreign_keys fk    
     inner    join sys.objects o1    
     on        fk.parent_object_id = o1.object_id    
     inner    join sys.schemas s1    
     on        o1.schema_id = s1.schema_id    
     inner    join sys.objects o2    
     on        fk.referenced_object_id = o2.object_id    
     inner    join sys.schemas s2    
     on        o2.schema_id = s2.schema_id    
     /*For the purposes of finding dependency hierarchy       
         we're not worried about self-referencing tables*/
     where    not    (    s1.schema_id = s2.schema_id
             and        o1.object_id = o2.object_id)
 )
 ,ordered_tables AS 
(        SELECT    s.schema_id as [schema_id]
         ,        t.object_id as table_id
         ,        0 AS Level    
         FROM    (    select    *                
                     from    sys.tables                 
                     where    name <> 'sysdiagrams') t    
         INNER    JOIN sys.schemas s    
         on        t.schema_id = s.schema_id    
         LEFT    OUTER JOIN fk_tables fk    
         ON        s.schema_id = fk.from_schema_id    
         AND        t.object_id = fk.from_table_id
         WHERE    fk.from_schema_id IS NULL
         UNION    ALL
         SELECT    fk.from_schema_id
         ,        fk.from_table_id
         ,        ot.Level + 1    
         FROM    fk_tables fk    
         INNER    JOIN ordered_tables ot    
         ON        fk.to_schema_id = ot.[schema_id]
         AND        fk.to_table_id = ot.table_id
 )
insert into #TableList
select distinct
schema_name(ot.[schema_id]) as SchemaName,
object_name(ot.table_id) as TableName,
ot.Level,
CASE WHEN fk2.referenced_object_id IS NULL THEN 1 ELSE 0 END as [IsLeafLevel]
from    ordered_tables ot
 inner    join (
         select    [schema_id]
         ,        table_id
         ,        MAX(Level) maxLevel        
         from    ordered_tables        
         group    by [schema_id],table_id
         ) mx
 on        ot.[schema_id]= mx.[schema_id]
 and        ot.table_id = mx.table_id
 and        mx.maxLevel = ot.Level
inner    join sys.schemas s1    
on        ot.[schema_id] = s1.schema_id 
left outer join sys.foreign_keys fk2
ON fk2.referenced_object_id = ot.table_id;

----

CREATE TABLE #BCPOut(id INT IDENTITY(1,1), ExportBCPStatements NVARCHAR(1000))
INSERT INTO #BCPOut
SELECT 'IF NOT EXIST "' + @Path +'\BCPData" MD "' + @Path +'\BCPData\"'
INSERT INTO #BCPOut
SELECT ' '

INSERT INTO #BCPOut
SELECT 'bcp [' + t.SchemaName + '].[' + t.TableName + '] out "' + @Path +'\BCPData\' + t.SchemaName + '.' +  
t.TableName + '.dat" -E -N -b 10000 -S ' + @SourceServer + ' -U ' + @SourceUser + ' -P ' + @SourcePwd + ' -d ' + @SourceDB + CHAR(13) + CHAR(10) +
'IF %ERRORLEVEL% NEQ 0 exit %ERRORLEVEL%'
FROM #TableList t
order by t.SchemaName,t.[Level],t.TableName;

INSERT INTO #BCPOut
SELECT ' '
INSERT INTO #BCPOut
SELECT 'ECHO Done exporting data'

CREATE TABLE BCPStatments(id int identity(1,1), Statements NVARCHAR(MAX))
INSERT INTO BCPStatments
SELECT ExportBCPStatements FROM #BCPOut ORDER BY id

SELECT @bcpString = 'bcp "SELECT Statements FROM ' + @SourceDB + '.dbo.BCPStatments ORDER BY id" queryout ' + @Path + '\Out.bat  -c -C1252 -S' + @SourceServer + ' -U' + @SourceUser + ' -P' + @SourcePwd
EXEC xp_cmdshell @bcpString
DROP TABLE BCPStatments

CREATE TABLE #BCPIn(id INT IDENTITY(1,1), ImportBCPStatements NVARCHAR(1000))

INSERT INTO #BCPIn
SELECT '@ECHO off'
INSERT INTO #BCPIn
SELECT ' '
INSERT INTO #BCPIn
SELECT 'IF NOT EXIST "' + @Path +'\BCPData" GOTO NOBCP'
INSERT INTO #BCPIn
SELECT 'IF EXIST "' + @Path +'\Logs" RD /S /Q "' + @Path +'\Logs\"'
INSERT INTO #BCPIn
SELECT 'IF NOT EXIST "' + @Path +'\Logs" MD "' + @Path +'\Logs\"'
INSERT INTO #BCPIn
SELECT ' '

INSERT INTO #BCPIn
SELECT 'bcp ' + t.SchemaName + '.' + t.TableName + ' in "' + @Path +'\BCPData\' + t.SchemaName + '.' +  
t.TableName + '.dat" -E -N -b 10000 -S ' + @DestServer + ' -U ' + @DestUser + ' -P ' + @DestPwd + ' -d ' + @DestDB +  
' -q' +
' -o "' + @Path +'\Logs\' + t.SchemaName + '.' + t.TableName + '.log"' + CHAR(13) + CHAR(10) + 'IF %errorlevel% EQU 0 Del "' + @Path +'\Logs\' + t.SchemaName + '.' +  t.TableName + '.log"'
FROM #TableList t
order by t.SchemaName,t.[Level],t.TableName;


INSERT INTO #BCPIn
SELECT ' '
INSERT INTO #BCPIn
SELECT 'GOTO Finish_OK'

INSERT INTO #BCPIn
SELECT ' '
INSERT INTO #BCPIn
SELECT ':NOBCP'
INSERT INTO #BCPIn
SELECT 'ECHO ---------'
INSERT INTO #BCPIn
SELECT 'ECHO You need BCP files in a BCPData folder to run this'
INSERT INTO #BCPIn
SELECT 'ECHO Import NOT Done'
INSERT INTO #BCPIn
SELECT 'ECHO ---------'
INSERT INTO #BCPIn
SELECT 'SET ERRORLEVEL=3'
INSERT INTO #BCPIn
SELECT 'GOTO Finish'
INSERT INTO #BCPIn
SELECT ' '

INSERT INTO #BCPIn
SELECT ':Finish_OK'
INSERT INTO #BCPIn
SELECT 'ECHO ---------'
INSERT INTO #BCPIn
SELECT 'ECHO Import OK!'
INSERT INTO #BCPIn
SELECT 'ECHO Look in folder "' + @Path +'\Logs\ for error files'
INSERT INTO #BCPIn
SELECT 'ECHO ---------'
INSERT INTO #BCPIn
SELECT 'GOTO Finish'
INSERT INTO #BCPIn
SELECT ' '

INSERT INTO #BCPIn
SELECT ':Finish'

CREATE TABLE BCPStatments(id int identity(1,1), Statements NVARCHAR(MAX))
INSERT INTO BCPStatments
SELECT ImportBCPStatements FROM #BCPIn ORDER BY id

SELECT @bcpString = 'bcp "SELECT Statements FROM ' + @SourceDB + '.dbo.BCPStatments ORDER BY id" queryout ' + @Path + '\In.bat  -c -C1252 -S' + @SourceServer + ' -U' + @SourceUser + ' -P' + @SourcePwd
EXEC xp_cmdshell @bcpString
DROP TABLE BCPStatments

DROP TABLE #TableList
DROP TABLE #BCPOut
DROP TABLE #BCPIn

