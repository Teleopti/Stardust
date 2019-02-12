IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateJobStartTime]')
   AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateJobStartTime]
GO

CREATE PROCEDURE [dbo].[UpdateJobStartTime]
	@businessunitId uniqueidentifier,
	@now datetime,
	@maxExecutionTime int
AS

declare
	@lockedStarttime datetime,
	@shouldProceed bit = 1,
	@returnValue bit = 0,
	@startTime datetime

BEGIN
SET NOCOUNT ON;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;  

Begin transaction

select @lockedStarttime = LockTimestamp,
	@startTime = starttime
from [SkillForecastJobStartTime]
where BusinessUnit = @businessunitid
if @startTime is null
	begin
		--print 'INSERt';
		insert into [SkillForecastJobStartTime] (BusinessUnit, StartTime, LockTimestamp) Values (@businessunitId,@now,DATEADD(MINUTE,@maxExecutionTime, @now))
	end
else
	begin
		--if lockedtimestamp is not null
		print @lockedStarttime

		if @now > @lockedStarttime or @lockedStarttime is null
			set @shouldProceed = 1
		else
			set @shouldProceed = 0
		
		if @shouldProceed =0 
			set @returnValue = 1
		else
			begin
				--print 'PROCEDDING NOW UPDATE';
				UPDATE [dbo].[SkillForecastJobStartTime] set LockTimestamp = DATEADD(MINUTE,@maxExecutionTime, @now), StartTime =  @now  WHERE BusinessUnit = @businessunitid
			end
	end

	--print @returnValue;

COMMIT TRANSACTION;

return @returnValue
END
GO