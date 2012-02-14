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
	SELECT DISTINCT p.person_code,al.datasource_id,al.acd_login_original_id FROM [mart].[bridge_acd_login_person] balp INNER JOIN mart.dim_person p ON p.person_id=balp.person_id INNER JOIN mart.dim_acd_login al ON balp.acd_login_id=al.acd_login_id WHERE p.person_code IS NOT NULL AND al.datasource_id<>-1
END

GO
