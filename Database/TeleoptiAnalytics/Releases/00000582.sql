IF NOT EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE  TABLE_NAME = 'fact_schedule'
                 AND COLUMN_NAME = 'planned_overtime_m') 
BEGIN
	ALTER TABLE mart.fact_schedule
	ADD planned_overtime_m int null
END

IF NOT EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE  TABLE_NAME = 'stg_schedule'
                 AND COLUMN_NAME = 'planned_overtime_m') 
BEGIN
	ALTER TABLE stage.stg_schedule
	ADD planned_overtime_m int null
END
