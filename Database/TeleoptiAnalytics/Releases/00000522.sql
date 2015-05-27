----------------  
--Name: David Jonsson
--Date: 2009-04-28
--Desc: bug #33313, ServiceBus is very slow starting - purge is not working within the timeout of 30 secs.
----------------
DELETE FROM Queue.Messages WHERE ExpiresAt < dateadd(DAY, -2, GetUtcDate()) 