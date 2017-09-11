IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[add_teleopti_firewall_rule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[add_teleopti_firewall_rule]
GO
