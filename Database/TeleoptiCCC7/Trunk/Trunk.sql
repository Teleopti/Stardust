----------------  
--Name: David J + Andreas S
--Date: 2012-00-11
--Desc: prepare Table for PS Tech custom SMS bridge
----------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[CustomTables].[SMS]') AND type in (N'U'))
BEGIN
	CREATE TABLE [CustomTables].[SMS](
		[Id] [uniqueidentifier] NOT NULL,
		[PhoneNumber] [nvarchar](50) NULL,
		[Msg] nvarchar(max) NULL,
		[RecivedTime] [datetime] NULL,
		[DeliveredTime] [datetime] NULL,
		[LastTriedTime] [datetime] NULL,
		[IsSent] [bit] NOT NULL DEFAULT 0,
		[ToBeSent] [bit] NOT NULL DEFAULT 1,
		[Status] [nvarchar](200) NOT NULL DEFAULT 'init'
		)
	           
	ALTER TABLE CustomTables.SMS ADD CONSTRAINT
		PK_SMS PRIMARY KEY CLUSTERED 
		(
		Id
		)
END
--Robin: Removing column IsDeleted as it isn't used anymore.

IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		EXECUTE('DELETE FROM dbo.StateGroupActivityAlarm WHERE IsDeleted=1')
	END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		ALTER TABLE dbo.StateGroupActivityAlarm	DROP COLUMN IsDeleted
	END
	
GO

--Anders: Adding possiblity to purge old messages cause it's been asked for
if not exists (select 1 from PurgeSetting where [Key] = 'Message')
begin
	insert into PurgeSetting values ('Message', 10)
end

----------------  
--Name: David J
--Date: 2012-09-26
--Desc: #18771 - Allowance bug, need to truncate readModel.ScheduleProjectionReadOnly
--		Since we are adding this on Trunk.sql => Add logic to truncate only once
----------------  
declare @bugId int
set @bugId=-17881
if not exists (select * from dbo.DatabaseVersion where BuildNumber=@bugId)
begin
	DECLARE @systemVersion varchar(100)
	SELECT TOP 1 @systemVersion=systemVersion FROM dbo.DatabaseVersion order by BuildNumber desc
	SELECT @systemVersion = @systemVersion + '.1'
	TRUNCATE TABLE readModel.ScheduleProjectionReadOnly
	INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (@bugId,@systemVersion)
end