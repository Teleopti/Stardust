ALTER TABLE [stage].[stg_person]
   ALTER COLUMN [windows_username] [nvarchar](100)
GO

ALTER TABLE [mart].[dim_person]
   ALTER COLUMN [windows_username] [nvarchar](100)
GO
