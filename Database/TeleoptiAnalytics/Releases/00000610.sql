-- Bug #48351
ALTER TABLE [stage].[stg_person]
   ALTER COLUMN [windows_domain] [nvarchar](100)
GO

ALTER TABLE [stage].[stg_person]
   ALTER COLUMN [windows_username] [nvarchar](100)
GO

ALTER TABLE [mart].[dim_person]
   ALTER COLUMN [windows_domain] [nvarchar](100)
GO

ALTER TABLE [mart].[dim_person]
   ALTER COLUMN [windows_username] [nvarchar](100)
GO

-- Bug #48496
ALTER TABLE [stage].[stg_overtime]
   ALTER COLUMN [overtime_name] nvarchar(250)
GO

ALTER TABLE [mart].[dim_overtime]
   ALTER COLUMN [overtime_name] nvarchar(250)
GO
