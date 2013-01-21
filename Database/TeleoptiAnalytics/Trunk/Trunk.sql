-- Robin Karlsson, 2013-01-17
-- Drop everything related to the message broker
--Stored Procedures
declare @sql varchar(max)
set @sql = (
select 'DROP PROCEDURE [' + routine_schema + '].[' + routine_name + '] ' 
from information_schema.routines where routine_schema = 'msg' and routine_type = 'PROCEDURE'
FOR XML PATH ('')
)
exec (@sql)

--views
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[msg].[vCurrentUsers]'))
DROP VIEW [msg].[vCurrentUsers]
GO

--tables
declare @sql varchar(max)
set @sql = (
select 'DROP TABLE [' + table_schema + '].[' + table_name + '] ' 
from information_schema.TABLES
where table_schema= 'msg' and table_type='BASE TABLE'
FOR XML PATH ('')
)
exec (@sql)
GO

--schema
IF  EXISTS (SELECT * FROM sys.schemas WHERE name = N'msg')
DROP SCHEMA [msg]
GO

