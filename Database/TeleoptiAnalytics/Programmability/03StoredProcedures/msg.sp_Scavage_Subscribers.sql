IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Scavage_Subscribers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Scavage_Subscribers]
GO


CREATE PROCEDURE [msg].[sp_Scavage_Subscribers]
(@SCAVAGE_DATETIME_WINDOW DATETIME)
AS
/*
declare @d datetime
select @d= dateadd(minute,-3,getdate())
select @d
exec [msg].[sp_Scavage_Subscribers] @d
*/
BEGIN
--Reset input to SQL Server time
SELECT
	@SCAVAGE_DATETIME_WINDOW = ISNULL(DATEADD(MILLISECOND,-CAST(ConfigurationValue as int),GETDATE()),180000)
FROM msg.configuration
WHERE ConfigurationId=10 --restartTimeSpan

INSERT INTO msg.Pending(SubscriberId) 
	SELECT a.SubscriberId FROM msg.Subscriber a
	WHERE not exists (SELECT 1 FROM msg.Heartbeat b where a.SubscriberId = b.SubscriberId)
IF(EXISTS(SELECT SubscriberId FROM msg.Pending GROUP BY SubscriberId HAVING COUNT(SubscriberId) > 4))
BEGIN
	DELETE FROM msg.Subscriber WHERE SubscriberId IN (SELECT SubscriberId FROM msg.Pending GROUP BY SubscriberId HAVING COUNT(SubscriberId) > 3)
	DELETE FROM msg.Filter WHERE SubscriberId IN (SELECT SubscriberId FROM msg.Pending GROUP BY SubscriberId HAVING COUNT(SubscriberId) > 3)
	DELETE msg.Filter FROM msg.Filter f
		WHERE NOT EXISTS (select 1 from msg.subscriber s where s.subscriberid = f.subscriberid)
		AND NOT EXISTS (select 1 from msg.pending p where p.subscriberid = f.subscriberid)
	DELETE FROM msg.Pending WHERE SubscriberId IN (SELECT SubscriberId FROM msg.Pending GROUP BY SubscriberId HAVING COUNT(SubscriberId) > 3)
	DELETE msg.Pending FROM msg.Pending p
		WHERE NOT EXISTS (select 1 from msg.filter f where p.subscriberid = f.subscriberid)
END
DELETE FROM msg.Heartbeat WHERE ChangedDateTime < @SCAVAGE_DATETIME_WINDOW
END

GO
