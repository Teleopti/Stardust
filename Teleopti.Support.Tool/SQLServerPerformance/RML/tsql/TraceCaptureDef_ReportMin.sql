/***************************************************
October 2008

This file is a lightweight trace definition with the minimum set of events
that will not fail ReadTrace processing.

Collects RPC Completed, Batch Completed, Recompiles, Most Errors & Warnings

Does not get execution plans.  Un-comment back in any additional events you want.

This capture works for reporting feature only and some reports may be limited in their detail.
For example since this does not capture statement level events, statement level detail and drill
through reports will not be available.

This capture will not work for any of the replay capability.

Change the InsertFileName here to a high speed local disk. Destination folder must already exist. 
Do not add the .TRC extension as the server will do that for you.

***************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'#tmpPPEventEnable') AND type in (N'P', N'PC'))
DROP PROCEDURE #tmpPPEventEnable
GO

create procedure #tmpPPEventEnable  @TraceID int, @iEventID int
as
begin
	set nocount on

	declare @iColID		int
	declare @iColIDMax	int
	declare @on bit

	set @on= 1
	set @iColID = 1
	set @iColIDMax = 64

	while(@iColID <= @iColIDMax)
	begin
		exec sp_trace_setevent @TraceID, @iEventID, @iColID, @on
		set @iColID = @iColID + 1
	end
end
GO

SET NOCOUNT ON
SET QUOTED_IDENTIFIER ON
---------------------
-- declare
---------------------
declare @rc int
declare @TraceID int
declare @maxfilesize bigint
declare @fileCount int
declare @MaxMinutesInt int
declare @stoptime datetime
declare @MaxDisc int
declare @FolderName nvarchar(1000)
declare @FileName nvarchar(2000)
DECLARE @DBIdString varchar(1000)

---------------------
-- Init
---------------------
set @maxfilesize = 250				--	An optimal size (Mb) for tracing and handling the files

---------------------
-- Get parameters from CmdShell
---------------------
select @MaxMinutesInt = '$(MaxMinutes)'	-- Stop trace after @MaxMinutes
select @FolderName = '$(FolderName)'	-- Folder name from batch
select @MaxDisc = '$(MaxDisc)'			-- maximum disc usage (Gb)
select @MaxMinutesInt = @MaxMinutesInt +1
select @DBIdString = '$(DBIdString)'	-- a comma seprated list of Database ids

--Set trace file name
declare @Now DATETIME
SET @Now = GETDATE()

SELECT @FileName =
		CAST(ISNULL(SERVERPROPERTY ('InstanceName'),SERVERPROPERTY ('MachineName')) AS VARCHAR(100))+
		' ' +
		CAST(datepart(YEAR, @Now) AS CHAR(4)) + 
        RIGHT('0'+LTRIM(cast(datepart(MONTH, @Now) as VARCHAR(2))), 2) + 
        RIGHT('0'+LTRIM(cast(datepart(DAY, @Now) as VARCHAR(2))), 2) +
        ' '+ 
        RIGHT('0'+LTRIM(cast(datepart(HOUR, @Now) as VARCHAR(2))), 2) + 
        RIGHT('0'+LTRIM(cast(datepart(MINUTE, @Now) as VARCHAR(2))), 2)

--add folder name
select @FileName = @FolderName + @FileName

--number of files to use
select @fileCount = @MaxDisc*1024/@maxfilesize 		--  Start to roll over trace files if more then this figure

--if it becomes 0, set 1
if @fileCount < 1
set @fileCount = 1

select @stoptime = dateadd(mi,@MaxMinutesInt,getdate())

exec @rc = sp_trace_create
	@TraceID		output,
	@options		= 2 /* rollover*/,
	@tracefile		= @FileName,
	@maxfilesize	= @maxfilesize,
	@stoptime		= @stoptime,
	@filecount		= @fileCount
	
if (@rc != 0) goto error

declare @off bit
set @off = 0

-- Set the events
exec #tmpPPEventEnable @TraceID, 10  --  RPC Completed
exec #tmpPPEventEnable @TraceID, 11  --  RPC Started

declare @strVersion varchar(10)

set @strVersion = cast(SERVERPROPERTY('ProductVersion') as varchar(10))
if( (select cast( substring(@strVersion, 0, charindex('.', @strVersion)) as int)) >= 9)
begin
	exec sp_trace_setevent @TraceID, 10, 1, @off		--		No Text for RPC, only Binary for performance
	exec sp_trace_setevent @TraceID, 11, 1, @off		--		No Text for RPC, only Binary for performance
end

--exec #tmpPPEventEnable @TraceID, 44  --  SP:StmtStarting
--exec #tmpPPEventEnable @TraceID, 45  --  SP:StmtCompleted
exec #tmpPPEventEnable @TraceID, 100 -- RPC Output Parameter

exec #tmpPPEventEnable @TraceID, 12  --  SQL Batch Completed
exec #tmpPPEventEnable @TraceID, 13  --  SQL Batch Starting
--exec #tmpPPEventEnable @TraceID, 40  --  SQL:StmtStarting
--exec #tmpPPEventEnable @TraceID, 41  --  SQL:StmtCompleted

exec #tmpPPEventEnable @TraceID, 17  --  Existing Connection
exec #tmpPPEventEnable @TraceID, 14  --  Audit Login
exec #tmpPPEventEnable @TraceID, 15  --  Audit Logout

--exec #tmpPPEventEnable @TraceID, 16  --  Attention

--exec #tmpPPEventEnable @TraceID, 19  --  DTC Transaction
--exec #tmpPPEventEnable @TraceID, 50  --  SQL Transaction
--exec #tmpPPEventEnable @TraceID, 50  --  SQL Transaction
--exec #tmpPPEventEnable @TraceID, 181 --  Tran Man Event
--exec #tmpPPEventEnable @TraceID, 182 --  Tran Man Event
--exec #tmpPPEventEnable @TraceID, 183 --  Tran Man Event
--exec #tmpPPEventEnable @TraceID, 184 --  Tran Man Event
--exec #tmpPPEventEnable @TraceID, 185 --  Tran Man Event
--exec #tmpPPEventEnable @TraceID, 186 --  Tran Man Event
--exec #tmpPPEventEnable @TraceID, 187 --  Tran Man Event
--exec #tmpPPEventEnable @TraceID, 188 --  Tran Man Event
--exec #tmpPPEventEnable @TraceID, 191 --  Tran Man Event
--exec #tmpPPEventEnable @TraceID, 192 --  Tran Man Event

--exec #tmpPPEventEnable @TraceID, 98  --  Stats Profile

--exec #tmpPPEventEnable @TraceID, 53  --  Cursor Open
--exec #tmpPPEventEnable @TraceID, 70  --  Cursor Prepare
--exec #tmpPPEventEnable @TraceID, 71  --  Prepare SQL
--exec #tmpPPEventEnable @TraceID, 73  --  Unprepare SQL
--exec #tmpPPEventEnable @TraceID, 74  --  Cursor Execute
--exec #tmpPPEventEnable @TraceID, 76  --  Cursor Implicit Conversion
--exec #tmpPPEventEnable @TraceID, 77  --  Cursor Unprepare
--exec #tmpPPEventEnable @TraceID, 78  --  Cursor Close

--exec #tmpPPEventEnable @TraceID, 22  --  Error Log
--exec #tmpPPEventEnable @TraceID, 25  --  Deadlock
--exec #tmpPPEventEnable @TraceID, 27  --  Lock Timeout
--exec #tmpPPEventEnable @TraceID, 60  --  Lock Escalation
--exec #tmpPPEventEnable @TraceID, 28  --  MAX DOP
--exec #tmpPPEventEnable @TraceID, 33  --  Exceptions
--exec #tmpPPEventEnable @TraceID, 34  --  Cache Miss
--exec #tmpPPEventEnable @TraceID, 37  --  Recompile
--exec #tmpPPEventEnable @TraceID, 39  --  Deprocated Events
--exec #tmpPPEventEnable @TraceID, 55  --  Hash Warning
--exec #tmpPPEventEnable @TraceID, 58  --  Auto Stats
--exec #tmpPPEventEnable @TraceID, 67  --  Execution Warnings
--exec #tmpPPEventEnable @TraceID, 69  --  Sort Warnings
--exec #tmpPPEventEnable @TraceID, 79  --  Missing Col Stats
--exec #tmpPPEventEnable @TraceID, 80  --  Missing Join Pred
--exec #tmpPPEventEnable @TraceID, 81  --  Memory change event
--exec #tmpPPEventEnable @TraceID, 92  --  Data File Auto Grow
--exec #tmpPPEventEnable @TraceID, 93  --  Log File Auto Grow
--exec #tmpPPEventEnable @TraceID, 116 --  DBCC Event
--exec #tmpPPEventEnable @TraceID, 125 --  Deprocation Events
--exec #tmpPPEventEnable @TraceID, 126 --  Deprocation Final
--exec #tmpPPEventEnable @TraceID, 127 --  Spills
--exec #tmpPPEventEnable @TraceID, 137 --  Blocked Process Threshold
--exec #tmpPPEventEnable @TraceID, 150 --  Trace file closed
--exec #tmpPPEventEnable @TraceID, 166 --  Statement Recompile
--exec #tmpPPEventEnable @TraceID, 196 --  CLR Assembly Load

--Filter on Database Id(s)
SELECT @DBIdString = REPLACE(@DBIdString,' ','')
DECLARE @DBId table (idx smallint Primary Key IDENTITY(1,1), Databaseb_Id int NOT NULL)
IF isnumeric(@DBIdString) = 1
begin
	INSERT INTO @DBId (Databaseb_Id)	
	SELECT Item = y.i.value('(./text())[1]', 'nvarchar(4000)')
	FROM 
	( 
		SELECT x = CONVERT(XML, '<i>' 
		+ REPLACE(@DBIdString, ',', '</i><i>') 
		+ '</i>').query('.')
	) AS a CROSS APPLY x.nodes('i') AS y(i)
end
-- enumerate the table @DBId
DECLARE @i int
DECLARE @And int
DECLARE @Or int
DECLARE @numrows int
DECLARE @Databaseb_Id int
SET @And = 0
SET @Or = 1
SET @i = 1
SET @numrows = (SELECT COUNT(*) FROM @DBId)
IF @numrows > 0
BEGIN
	WHILE (@i <= (SELECT MAX(idx) FROM @DBId))
	BEGIN

		-- get the next database_id by primary key
		SET @Databaseb_Id = (SELECT Databaseb_Id FROM @DBId WHERE idx = @i)

		if @i = 1 --first condition = AND
			exec sp_trace_setfilter @TraceID, 3, @And, 0, @Databaseb_Id
		else
			exec sp_trace_setfilter @TraceID, 3, @Or, 0, @Databaseb_Id

		-- increment counter
		SET @i = @i + 1
	END
END


-- SELECT @TraceID back to callint batch file
print cast(@TraceID as nvarchar(5)) + ',"' + @FileName + '"'
SET NOCOUNT OFF

goto finish
/*
exec sp_trace_setstatus @traceid=2, @status=0
exec sp_trace_setstatus @traceid=2, @status=2

Status:
0 = Stops the specified trace.
1 = Starts the specified trace.
2 = Closes the specified trace and deletes its definition from the server.

--check running traces. Note traceid=1 is a system trace
SELECT * FROM ::fn_trace_getinfo(default)

*/
/*
print '--------'
print '--Get running traces:'
print 'SELECT * FROM :: fn_trace_getinfo(default)'
print '--------'
print '--Select data from current trace:'
print 'SELECT * FROM ::fn_trace_gettable('''+@FileName+'.trc'', DEFAULT)'
print '--------'
*/
/*
print '--'+@FileName
print '--Issue the following commands (t-sql) if you need to stop the trace manually:'
print 'exec sp_trace_setstatus ' + cast(@TraceID as varchar) + ', 0'
print 'go'
print 'exec sp_trace_setstatus ' + cast(@TraceID as varchar) + ', 2'
*/

error: 


finish: 
--select * from ::fn_trace_geteventinfo(@TraceID)
--select * from sys.traces
--:exit(select @rc)
go