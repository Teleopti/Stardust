--Fix data after baseline restore
--:SETVAR ccc7 VMDemoreg_TeleoptiCCC7
--:SETVAR mart VMDemoreg_TeleoptiAnalytics

---------------------------
--fix cross db views
---------------------------
EXEC $(mart).mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '$(agg)'
EXEC $(mart).mart.sys_crossDatabaseView_load

---------------------------
--delete current mart permissions
---------------------------
delete $(mart).mart.pm_user
delete $(mart).dbo.aspnet_Membership
delete $(mart).dbo.aspnet_Users
delete $(mart).mart.permission_report

---------------------------
--update MsgBroker settings
---------------------------
update $(mart).msg.configuration
set ConfigurationValue='Teleopti729'
where ConfigurationName='Server'

update $(mart).msg.configuration
set ConfigurationValue='9080'
where ConfigurationName='Port'

update $(mart).msg.configuration
set ConfigurationValue='ClientTcpIp'
where ConfigurationName='MessagingProtocol'

update $(mart).msg.address
set Address='Teleopti729',Port=9080
where addressId=1

---------------------------
--set same password on all user="testpwd", disable Windows login
---------------------------
update $(ccc7).dbo.Person
set
	windowsLogonName='',
	DomainName='',
	PartOfUnique=Id,
	password='###07A68F077FBB9CA312256A51FF87B6D97A6F7E39###' --testpwd

/*
select p.ApplicationLogOnName,',testpwd' from PersonPeriodWithEndDate pp
inner join Person p
on p.Id = pp.Parent
where getdate() between pp.StartDate and pp.EndDate
and ApplicationLogOnName <> ''
and p.IsDeleted=0
*/