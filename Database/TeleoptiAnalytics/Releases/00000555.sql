update [mart].[dim_day_off] set day_off_code = '00000000-0000-0000-0000-000000000000'
where day_off_id = -1

IF NOT EXISTS(SELECT 1 FROM [mart].[dim_day_off] where [day_off_code] is null) 
	AND NOT EXISTS(SELECT 1 FROM [mart].[dim_day_off] group by day_off_code having count(1) > 1)
BEGIN
	ALTER TABLE [mart].[dim_day_off] ALTER COLUMN [day_off_code] UNIQUEIDENTIFIER NOT NULL
	IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME ='AK_day_off_code')
	BEGIN
		ALTER TABLE [mart].[dim_day_off] ADD CONSTRAINT AK_day_off_code UNIQUE (day_off_code)
	END
END