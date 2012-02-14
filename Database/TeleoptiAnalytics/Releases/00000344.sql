----------------  
--Name: Jonas n
--Date: 2011-11-16  
--Desc: Add configuration table
----------------  
CREATE TABLE [mart].[sys_configuration](
	[key] [nvarchar](50) NOT NULL,
	[value] [nvarchar](200) NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_sys_configuration] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)
) 
GO

ALTER TABLE [mart].[sys_configuration] ADD  CONSTRAINT [DF_sys_configuration_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (344,'7.1.344') 
