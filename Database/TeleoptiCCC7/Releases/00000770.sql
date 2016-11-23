--#remove the temp fix added for some customers
IF OBJECT_ID('mart.[UC_dim_date_date_date]', 'UQ') IS NOT NULL 
    ALTER TABLE [mart].[dim_date] DROP CONSTRAINT [UC_dim_date_date_date]

ALTER TABLE [mart].[dim_date] ADD CONSTRAINT UK_dim_date_date_date UNIQUE (date_date)
GO