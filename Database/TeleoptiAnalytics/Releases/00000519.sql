exec('CREATE SCHEMA [msg] AUTHORIZATION [dbo]')

ALTER SCHEMA msg 
    TRANSFER dbo.Mailbox;

ALTER SCHEMA msg 
    TRANSFER dbo.[Notification];