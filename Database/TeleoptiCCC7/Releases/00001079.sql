UPDATE [Tenant].[ServerConfiguration]
   SET [Value] = 'https://api.teleopticloud.com/TeleoptiNotifications/Subscription'
 WHERE [Tenant].[ServerConfiguration].[Key] = 'NotificationSubscriptionApiEndpoint'
GO

UPDATE [Tenant].[ServerConfiguration]
   SET [Value] = 'a431255e4eb842f89615a093e2abd77d'
 WHERE [Tenant].[ServerConfiguration].[Key] = 'NotificationSubscriptionApiEndpointKey'
GO