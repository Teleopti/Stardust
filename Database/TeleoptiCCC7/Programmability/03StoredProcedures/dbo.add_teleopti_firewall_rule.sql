IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[add_teleopti_firewall_rule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[add_teleopti_firewall_rule]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2017-07-11
-- Update date: 
--				yyyy-mm-dd Some comment
-- Description:	Make it possible to add a firewall rule with limited persmissions
-- Example call: EXEC [dbo].[add_teleopti_firewall_rule] @ip_address='65.132.155.83', @personId='A2D29585-C3F3-47FF-86EF-BC4CAB5087CC'
-- =============================================
CREATE PROC [dbo].[add_teleopti_firewall_rule]
@ip_address varchar(50),
@personId uniqueidentifier

WITH EXECUTE AS OWNER
AS
SET NOCOUNT ON

--if not Azure exit out
if (select [dbo].[IsAzureDB]()) = 0
RETURN 0

--init
declare @ruleName nvarchar(128) = ''

--clean up old entries (>30 days)
DECLARE IP_cursor CURSOR FOR  
select [Name] from sys.database_firewall_rules where dateadd(day,30,create_date) > getutcdate()
OPEN IP_cursor   
FETCH NEXT FROM IP_cursor INTO @ruleName
	WHILE @@FETCH_STATUS = 0   
	BEGIN   
		EXECUTE sp_delete_database_firewall_rule @name=@ruleName
		FETCH NEXT FROM IP_cursor INTO @ruleName
	END   
CLOSE IP_cursor   
DEALLOCATE IP_cursor

--Add new IP, if needed
set @ruleName = N'SmartClientPortal_' + @ip_address + '_' + cast(@personId as varchar(36))
if not exists (select 1 from sys.database_firewall_rules where start_ip_address = @ip_address)
begin
	execute sp_set_database_firewall_rule @name=@ruleName, @start_ip_address=@ip_address, @end_ip_address=@ip_address
end
RETURN 0
GO