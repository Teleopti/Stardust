IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME ='AK_day_off_code')
BEGIN
	ALTER TABLE [mart].[dim_day_off] DROP CONSTRAINT [AK_day_off_code]
END

UPDATE mart.dim_day_off
SET day_off_code = '00000000-0000-0000-0000-000000000000'
WHERE day_off_name = 'ContractDayOff' 
and day_off_shortname = 'CD'
and day_off_code<>'00000000-0000-0000-0000-000000000000'

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME ='AK_day_off_code_business_unit_id')
BEGIN
	ALTER TABLE [mart].[dim_day_off] ADD  CONSTRAINT [AK_day_off_code_business_unit_id] UNIQUE NONCLUSTERED 
	(
		[day_off_code] ASC,
		[business_unit_id] ASC
	)
END