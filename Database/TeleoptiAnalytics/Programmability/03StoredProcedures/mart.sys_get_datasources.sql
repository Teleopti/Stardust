IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[sys_get_datasources]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[sys_get_datasources]
GO


-- =============================================
-- Author:		JN
-- Create date: 2008-02-21
-- Description:	Returns all the datasources except for 'Not Defined'
--
-- ChangeLog
-- Date			By	Description
---==============================================
-- 2008-09-31	DJ	Added internal values in sys_datasoruce (sys_crossdatabaseview)
-- 2008-10-03	DJ	Reverted back. Cross db vies removed from sys_datasource
-- 2011-02-11	DJ	Use ccc_system_info instead = the common Agg interval length. (was each log object original interval length, prior to transformation)
-- 2011-05-26	DJ	#15114 - Use id instead of string to get intervals per day
-- 2011-07-18	DJ	Adding internal agg
-- =============================================
CREATE PROCEDURE [mart].[sys_get_datasources]
@get_valid_datasource bit,
@include_option_all bit
AS
BEGIN
	
	SET NOCOUNT ON;
CREATE TABLE #sys_datasource(
	[datasource_id] [smallint] NOT NULL,
	[datasource_name] [nvarchar](100) NULL,
	[time_zone_id] [int] NULL,
	[time_zone_code] [nvarchar](50) NULL,
	[interval_length] [int] NULL,
	[inactive] [bit] NOT NULL,
	[sorting] int NULL
)

    IF @get_valid_datasource = 1
	BEGIN

		-- Get only valid datasources with time zone set
		INSERT INTO #sys_datasource
		SELECT
			sd.datasource_id,
			sd.datasource_name, 
			sd.time_zone_id,
			tz.time_zone_code,
			1440 / si.int_value 'interval_length',
			sd.inactive,
			1
		FROM mart.sys_datasource sd
			INNER JOIN mart.v_ccc_system_info si
				ON si.[id] = 1 --hardcoded key for "CCC intervals per day"
			INNER JOIN mart.dim_time_zone tz
				ON sd.time_zone_id = tz.time_zone_id
		WHERE sd.datasource_id NOT IN (-1,1)
			AND sd.time_zone_id IS NOT NULL
			AND sd.inactive = 0

		IF @include_option_all = 1
		BEGIN
			--Prepare test for <All log Objects>
			DECLARE @countActive int
			DECLARE @countTotal int
					
			--Get all active log objects
			SELECT @countTotal = COUNT(*) FROM mart.sys_datasource sd WHERE datasource_id NOT IN (1,-1) AND inactive=0
			
			--Count all - configured and active
			SELECT @countActive = COUNT(*) FROM mart.sys_datasource sd WHERE datasource_id NOT IN (1,-1) AND inactive=0 AND time_zone_id IS NOT NULL
			
			--check that we do have log objects, they are active
			IF	(@countTotal = @countActive)
			BEGIN
				INSERT INTO #sys_datasource
				SELECT
					cast(-2 as smallint) as 'datasource_id',
					N'<All>' as 'datasource_name',
					cast(0 as smallint) as 'time_zone_id',
					N'UTC' as 'time_zone_code',
					15 as 'interval_length',
					cast(0 as bit) as 'inactive',
					0
			END
		END
				
		SELECT
			[datasource_id],
			[datasource_name],
			[time_zone_id],
			[time_zone_code],
			[interval_length],
			[inactive]
		FROM #sys_datasource
		ORDER BY sorting, datasource_name,datasource_id

	END
	ELSE
		BEGIN
		-- Get only invalid datasources with NO time zone set
		SELECT sd.datasource_id,
			sd.datasource_name, 
			-1 'time_zone_id',
			NULL 'time_zone_code',
			1440 / si.int_value 'interval_length',
			sd.inactive
		FROM mart.sys_datasource sd
			INNER JOIN mart.v_ccc_system_info si
				ON si.[id] = 1 --hardcoded key for "CCC intervals per day"
		WHERE sd.datasource_id NOT IN (-1,1)
			AND sd.time_zone_id IS NULL
			AND sd.inactive = 0
		ORDER BY sd.datasource_name, sd.datasource_id
	END

END


GO

