--Adding current user to Standard demo-user
DECLARE @csv varchar(100)
DECLARE @userid uniqueidentifier

SET @userid = '10957ad5-5489-48e0-959a-9b5e015b2b5c'
SELECT @csv=system_user

--delete all Windows domains as they stall IIS -> AD-lookup in TeleoptiPM
DELETE FROM TeleoptiCCC7_Demo.dbo.AuthenticationInfo

--insert current user and connect to @userid
INSERT INTO TeleoptiCCC7_Demo.dbo.AuthenticationInfo
SELECT
	Person=@userid,
	[Identity]=@csv

--Add currect user to IIS-users: update aspnet_users
UPDATE TeleoptiAnalytics_Demo.dbo.aspnet_Users
SET UserName=system_user,LoweredUserName=system_user
WHERE userid=@userid

--flush old RTA AcutalAgentState, else report can't handle seconds more than 24 hours
TRUNCATE TABLE TeleoptiAnalytics_Demo.rta.ActualAgentState  