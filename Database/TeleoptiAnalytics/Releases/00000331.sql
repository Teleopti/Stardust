/* 
Trunk initiated: 
2011-06-27 
11:26
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2011-07-18
--Desc: Dual agg-databases (external / internal)
----------------
--Add with default constraint since ETL-tool is not aware of this property (get;set;)
ALTER TABLE mart.sys_datasource ADD
	internal bit NOT NULL
	CONSTRAINT DF_sys_datasource_internal DEFAULT 0
GO

--Azure log objects are always internal
DECLARE @sqlEdition NVARCHAR(200)
SELECT @sqlEdition = CONVERT(NVARCHAR(200), SERVERPROPERTY('edition'))

IF @sqlEdition = 'SQL Azure'
UPDATE mart.sys_datasource 
SET internal = 1
WHERE datasource_id NOT IN (-1,1)
GO

--Tables we  will need for fetchning data from logDB
-----------------
--acd_type_detail
-----------------
CREATE TABLE [dbo].[acd_type_detail](
	[acd_type_id] [int] NOT NULL,
	[detail_id] [int] NOT NULL,
	[detail_name] [varchar](50) NOT NULL,
	[proc_name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_acd_type_detail] PRIMARY KEY CLUSTERED 
(
	[acd_type_id] ASC,
	[detail_id] ASC
)
)

ALTER TABLE [dbo].[acd_type_detail]  WITH CHECK ADD  CONSTRAINT [FK_acd_type_detail_acd_type] FOREIGN KEY([acd_type_id])
REFERENCES [dbo].[acd_type] ([acd_type_id])

ALTER TABLE [dbo].[acd_type_detail] CHECK CONSTRAINT [FK_acd_type_detail_acd_type]
GO

-----------------
--log_object_add_hours
-----------------
CREATE TABLE [dbo].[log_object_add_hours](
	[log_object_id] [int] NOT NULL,
	[datetime_from] [smalldatetime] NOT NULL,
	[datetime_to] [smalldatetime] NOT NULL,
	[add_hours] [int] NOT NULL,
 CONSTRAINT [PK_log_object_add_hours] PRIMARY KEY CLUSTERED 
(
	[log_object_id] ASC,
	[datetime_from] ASC
)
)

ALTER TABLE [dbo].[log_object_add_hours]  WITH CHECK ADD  CONSTRAINT [FK_log_object_add_hours_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])

ALTER TABLE [dbo].[log_object_add_hours] CHECK CONSTRAINT [FK_log_object_add_hours_log_object]
GO

-----------
--Remove obsolet SP
-----------
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_interval_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_interval_get]
GO

----------------  
--Name: Anders F  
--Date: 2010-08-15  
--Desc: We can't afford to idle 5 ms  between each subscriber. Set ServerThrottle to 0!
----------------  

update msg.Configuration
set ConfigurationValue = 0
where ConfigurationId = 12
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (331,'7.1.331') 
