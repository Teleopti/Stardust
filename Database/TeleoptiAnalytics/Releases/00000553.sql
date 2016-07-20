ALTER TABLE stage.stg_day_off
ALTER COLUMN day_off_code uniqueidentifier NOT NULL

ALTER TABLE stage.stg_day_off DROP CONSTRAINT PK_stg_day_off
ALTER TABLE stage.stg_day_off ADD CONSTRAINT PK_stg_day_off PRIMARY KEY(day_off_code, day_off_name, business_unit_code)

GO