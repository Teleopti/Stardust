DECLARE @parent UNIQUEIDENTIFIER,
	@id UNIQUEIDENTIFIER,
	@administratorId UNIQUEIDENTIFIER
SELECT @parent = id FROM  [dbo].[ApplicationFunction] WHERE FunctionCode = 'WebRequests' and FunctionDescription = 'xxRequests'
SET @id = 'F31B4331-C0DF-4D7C-A109-A6DB8D0B544E'
SET @administratorId = '3F0886AB-7B25-4E95-856A-0D726EDC2A67'

DELETE FROM [dbo].[ApplicationFunction] WHERE id = @id

INSERT INTO [dbo].[ApplicationFunction]
           ([Id]
           ,[Version]
           ,[UpdatedBy]
           ,[UpdatedOn]
           ,[Parent]
           ,[FunctionCode]
           ,[FunctionDescription]
           ,[ForeignId]
           ,[ForeignSource]
           ,[IsDeleted])
     VALUES
           (@id
           ,1
           ,@administratorId
           ,GETUTCDATE()
           ,@parent
           ,'OvertimeRequestWeb'
           ,'xxOvertimeRequests'
           ,'0149'
           ,'Raptor'
           ,0)