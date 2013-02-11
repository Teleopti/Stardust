----------------  
--Name: CS
--Date: 2013-02-11  
--Desc: PBI #22024 
----------------  

-- Add column for activity preference

ALTER TABLE stage.stg_schedule_preference ADD
	activity_code uniqueidentifier NULL
GO

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

