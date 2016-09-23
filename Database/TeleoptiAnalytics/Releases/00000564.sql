--change  acd_login_original_id from nvarchar(50) to nvarchar(65)
ALTER TABLE [mart].[dim_acd_login] ALTER COLUMN [acd_login_original_id] nvarchar(65)
ALTER TABLE [stage].[stg_acd_login_person] ALTER COLUMN [acd_login_code] nvarchar(65)

UPDATE [mart].[dim_acd_login]
SET acd_login_original_id=SUBSTRING(acd_login_original_id,1,LEN(acd_login_original_id)-11),
acd_login_name=SUBSTRING(acd_login_name,1,LEN(acd_login_name)-11),
[is_active]=1
FROM [mart].[dim_acd_login]
WHERE is_active=0
AND RIGHT(acd_login_original_id,11)=' (inactive)'