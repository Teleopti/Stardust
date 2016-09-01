--#40372 Clear out old succeded Jobs in Hangifre
while 1=1
begin
	delete top(100) from [HangFire].[Job] with (readpast) where ExpireAt < getdate()
	if @@rowcount = 0 break
end