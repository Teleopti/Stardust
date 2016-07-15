/*

--=====
NOTE: Do NOT(!) run this on your PRODUCTION database, that will drop all users!
--=====

--=====
--manuell instructions
--=====
Run this script in your restored App database:
e.g
Bug28762_Acme_TeleoptiApp

This script will update the tenant info in your db copy to point to MySelf, (rather that poiting back to the production DB)

--Remove $ and () and replace with actuall name of your Analytics databases (between the single quots)
--Same for Username and password
--For Example:
declare @DESTANALYTICS VARCHAR(100) = 'IkeaTest_TeleoptiAnalytics' -- NOTE: Should always be Anayltics, even when executing in App database
declare @DESTUSER VARCHAR(100) = 'SomeUser'
declare @DESTPWD VARCHAR(100) = 'SomePassword'
*/

declare @DESTANALYTICS VARCHAR(100) = '$(DESTANALYTICS)' -- <-- Edit me e.g: Bug28762_Acme_TeleoptiAnalytics.
declare @DESTUSER VARCHAR(100) = '$(DESTUSER)'  -- < -- put your new debug SQL login here
declare @DESTPWD VARCHAR(100) = '$(DESTPWD)' -- < -- put your new debug SQL password here
--========================

SET NOCOUNT ON
print '---'
declare @ApplicationConnectionString nVARCHAR(200)
declare @AnalyticsConnectionString nVARCHAR(200)

select @ApplicationConnectionString = 'Data Source='+@@servername+'.database.windows.net;Initial Catalog='+DB_NAME()+';User ID='+@DESTUSER+';Password='+@DESTPWD+';Current Language=us_english'
select @AnalyticsConnectionString = 'Data Source='+@@servername+'.database.windows.net;Initial Catalog='+@DESTANALYTICS+';User ID='+@DESTUSER+';Password='+@DESTPWD+';Current Language=us_english'

update tenant.tenant
set
	ApplicationConnectionString=@ApplicationConnectionString,
	AnalyticsConnectionString=@AnalyticsConnectionString
where Name = 'Teleopti WFM'
