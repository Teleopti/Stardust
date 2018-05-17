UPDATE msg.Configuration 
SET ConfigurationValue = 'MESSAGE_BROKER_PORT' 
WHERE ConfigurationName = 'Port'

UPDATE msg.Configuration 
SET ConfigurationValue = 'MESSAGE_BROKER_IP' 
WHERE ConfigurationName = 'Server'

--Not needed any more
--UPDATE msg.Configuration 
--SET ConfigurationValue = 'XXXCONN_STR;Initial Catalog=XXXDATABASE;Application name=Teleopti.MessageBroker' 
--WHERE ConfigurationName = 'ConnectionString'

UPDATE [msg].[Address]
SET Address = 'MESSAGE_BROKER_IP'
WHERE AddressId = 1