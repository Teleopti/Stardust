ALTER TABLE [stage].[stg_time_zone]
ADD utc_in_use bit NOT NULL DEFAULT(0)

ALTER TABLE [mart].[dim_time_zone]
ADD utc_in_use bit NOT NULL DEFAULT(0)

