/* 
Trunk initiated: 
2009-12-07 
08:24
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: JN
--Date: 2009-12-08
--Desc: Add inactive column to mart.sys_datasource.(Should have been checked in before last built...)
----------------  
ALTER TABLE mart.sys_datasource
           DROP CONSTRAINT DF_sys_datasource_log_object_id
GO
ALTER TABLE mart.sys_datasource
           DROP CONSTRAINT DF_sys_datasource_datasource_type_name
GO
ALTER TABLE mart.sys_datasource
           DROP CONSTRAINT DF_sys_datasource_insert_date
GO
ALTER TABLE mart.sys_datasource
           DROP CONSTRAINT DF_sys_datasource_update_date
GO
CREATE TABLE mart.Tmp_sys_datasource
           (
           datasource_id smallint NOT NULL IDENTITY (1, 1),
           datasource_name nvarchar(100) NULL,
           log_object_id int NULL,
           log_object_name nvarchar(100) NULL,
           datasource_database_id int NULL,
           datasource_database_name nvarchar(100) NULL,
           datasource_type_name nvarchar(100) NULL,
           time_zone_id int NULL,
           inactive bit NOT NULL,
           insert_date smalldatetime NULL,
           update_date smalldatetime NULL
           )  ON MART
GO
ALTER TABLE mart.Tmp_sys_datasource ADD CONSTRAINT
           DF_sys_datasource_log_object_id DEFAULT ((-1)) FOR log_object_id
GO
ALTER TABLE mart.Tmp_sys_datasource ADD CONSTRAINT
           DF_sys_datasource_datasource_type_name DEFAULT ('Not Defined') FOR datasource_type_name
GO
ALTER TABLE mart.Tmp_sys_datasource ADD CONSTRAINT
           DF_sys_datasource_inactive DEFAULT 0 FOR inactive
GO
ALTER TABLE mart.Tmp_sys_datasource ADD CONSTRAINT
           DF_sys_datasource_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_sys_datasource ADD CONSTRAINT
           DF_sys_datasource_update_date DEFAULT (getdate()) FOR update_date
GO
SET IDENTITY_INSERT mart.Tmp_sys_datasource ON
GO
IF EXISTS(SELECT * FROM mart.sys_datasource)
            EXEC('INSERT INTO mart.Tmp_sys_datasource (datasource_id, datasource_name, log_object_id, log_object_name, datasource_database_id, datasource_database_name, datasource_type_name, time_zone_id, insert_date, update_date)
                      SELECT datasource_id, datasource_name, log_object_id, log_object_name, datasource_database_id, datasource_database_name, datasource_type_name, time_zone_id, insert_date, update_date FROM mart.sys_datasource WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT mart.Tmp_sys_datasource OFF
GO
DROP TABLE mart.sys_datasource
GO
EXECUTE sp_rename N'mart.Tmp_sys_datasource', N'sys_datasource', 'OBJECT' 
GO
ALTER TABLE mart.sys_datasource ADD CONSTRAINT
           PK_sys_datasource PRIMARY KEY CLUSTERED 
           (
           datasource_id
           ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO

 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (179,'7.0.179') 
