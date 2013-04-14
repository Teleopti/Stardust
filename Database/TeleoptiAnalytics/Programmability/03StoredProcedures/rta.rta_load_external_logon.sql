IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_load_external_logon]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_load_external_logon]
GO

-- =============================================
-- Author:		Robin K
-- Create date: 2011-08-15
-- Description:	Load external log on for use in RTA
-- =============================================
CREATE PROCEDURE [RTA].[rta_load_external_logon] AS
BEGIN
	--get current date id
	declare @toDayDateonly	smalldatetime
	declare @toDayDateId	int
	declare @maxDateId	int
	set @toDayDateonly = DATEADD(dd, 0, DATEDIFF(dd, 0, GETUTCDATE()))
	select @toDayDateId = date_id from mart.dim_date where date_date = @toDayDateonly
	select @maxDateId = max(date_id) from mart.dim_date where date_id > 0
	
	--todo: raiserror instead?
	--in case dim_date is not loaded for "today", pick max date
	select @toDayDateId = ISNULL(@toDayDateId,@maxDateId)

	SELECT DISTINCT p.person_code,al.datasource_id,al.acd_login_original_id,p.business_unit_code
	FROM [mart].[bridge_acd_login_person] balp
	INNER JOIN mart.dim_person p
		ON p.person_id=balp.person_id
	INNER JOIN mart.dim_acd_login al
		ON balp.acd_login_id=al.acd_login_id
	WHERE p.person_code IS NOT NULL
	AND al.datasource_id<>-1
	AND	( -- person periods "now", but extend an bit for timezone safty
			(@toDayDateId between (p.valid_from_date_id-1) and (p.valid_to_date_id+1))
			or 
			(@toDayDateId > (p.valid_from_date_id-1) and p.valid_to_date_id=-2)
		)
END

GO