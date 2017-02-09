--#remove the temp fix added for some customers
IF EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='UC_dim_date_date_date')
BEGIN
	ALTER TABLE [mart].[dim_date] DROP CONSTRAINT [UC_dim_date_date_date]
END

ALTER TABLE [mart].[dim_date] ADD CONSTRAINT [UC_dim_date_date_date] UNIQUE (date_date)
GO