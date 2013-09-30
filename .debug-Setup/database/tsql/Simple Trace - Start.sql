/*
2008-09-29
Teleopti AB, David Jonsson
With thanks to Allan Lyhre
sp_trace_setstatus 2, 0
GO
sp_trace_setstatus 2, 2
*/
SET NOCOUNT ON

-- Declare variables
DECLARE	@FilePath		AS NVARCHAR(4000)					-- File path to store the trace file
DECLARE	@MaxFileSize	AS BIGINT							-- Maximum size (in MB) of the trace file
DECLARE @hours			INT									-- Number of hours to run the trace
DECLARE	@TraceID		AS INTEGER							-- Returns the ID of the created trace (optional)
DECLARE	@FileName		AS NVARCHAR(4000)					-- Returns the name of the created trace file (optional)
DECLARE @tEvent			TABLE(EventNumber INT)				-- Table that holds the EventClass we will trace
DECLARE	@StopTime		AS DATETIME							
DECLARE	@Onflag			BIT
DECLARE @EventNumber	INT
DECLARE @ColumnNumber	INT


-- Initialize variables
SET @MaxFileSize	= 1000 --Mb
SET @FileName		= '$(FileName)'
SET @hours			= 12 --Run for 12 hours
SET	@StopTime		= DATEADD(Hour, @hours, CURRENT_TIMESTAMP)	
SET @Onflag			= 1

--Init the events to trace
--Starts a trace that captures the RPC:Starting (11), RPC:Completed (10), SQL:BatchStarting (13),
--SQL:BatchCompleted (12), Blocked Process Report (137) and Deadlock Graph (148) events.

INSERT INTO @tEvent
VALUES (10)
INSERT INTO @tEvent
VALUES (11)
INSERT INTO @tEvent
VALUES (12)
INSERT INTO @tEvent
VALUES (13)

--Optional: EventClass 137
--Note: to track Blocked Process Report (137), you first need to define: sp_configure 'blocked process threshold', [value int]
--http://msdn.microsoft.com/en-us/library/ms181150(SQL.90).aspx

--INSERT INTO @tEvent
--VALUES (137)

--Optional: EventClass 148
--Note: To get data from EventClass 148, you first need to enable the deadlock event
--Please turn this one off after finishing the trace! Else it will be configured until SQL Server is re-started
--DBCC TRACEON (1222, -1)


--INSERT INTO @tEvent
--VALUES (148)

BEGIN TRY

	-- Remove any trailing backslash from the filepath
--	SET @FilePath		= LTRIM(@FilePath)
--	IF (RIGHT(@FilePath, 1) = '\') SET @FilePath = SUBSTRING(@FilePath, 1, LEN(@FilePath) - 1)

	-- Create the filename for the trace file
--	SET @FileName	= @FilePath + '\'
--					+ CAST(SERVERPROPERTY('MachineName') AS VARCHAR) + '_'
--					+ ISNULL(CAST(SERVERPROPERTY('InstanceName') AS VARCHAR), 'MSSQL') + '_'
--					+ CONVERT(VARCHAR, CURRENT_TIMESTAMP, 112)
--					+ REPLACE(CONVERT(VARCHAR, CURRENT_TIMESTAMP, 108), ':', '')

	-- Create temporary tables
	CREATE TABLE #tFreeSpace 
	( 
		DriveLetter	CHAR(1), 
		FreeSpace	BIGINT
	)

	-- Check if there is enough free space on destination drive
	INSERT INTO #tFreeSpace EXEC master.dbo.xp_fixeddrives 

	IF (SELECT FreeSpace FROM #tFreeSpace WHERE DriveLetter = LEFT(@FilePath, 1)) < ISNULL(@MaxFileSize, 0)
		RAISERROR('Not enough free space on destination drive.', 16, 1)

 	--Drop  #tFreeSpace
	DROP TABLE #tFreeSpace

	-- Create the directory
--	EXEC master.dbo.xp_create_subdir @FilePath

	-- Create a new trace
	EXEC sp_trace_create @TraceID output, 0, @FileName, @MaxFileSize, @StopTime 

	-- Create a cursor for all events that are to be traced
	DECLARE Event_cursor CURSOR FOR
		SELECT EventNumber FROM @tEvent
	
	OPEN Event_cursor
	FETCH NEXT FROM Event_cursor INTO @EventNumber
	
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @ColumnNumber = 1

		-- Add columns 1 to 64 for the current event to the trace
		-- (see sp_trace_setevent in BOL for details)
		WHILE @ColumnNumber <= 64		
		BEGIN

			EXEC sp_trace_setevent @TraceID, @EventNumber, @ColumnNumber, @Onflag
			SET @ColumnNumber = @ColumnNumber + 1

		END
	
		FETCH NEXT FROM Event_cursor INTO @EventNumber
	END
	
	CLOSE Event_cursor 
	DEALLOCATE Event_cursor 

	-- Set the Filters
/*
	declare @intfilter int
	declare @bigintfilter bigint

	set @intfilter = 132
	exec sp_trace_setfilter @TraceID, 3, 1, 0, @intfilter

	set @intfilter = 133
	exec sp_trace_setfilter @TraceID, 3, 1, 0, @intfilter

	set @intfilter = 134
	exec sp_trace_setfilter @TraceID, 3, 1, 0, @intfilter
*/
	-- Set the trace status to start
	EXEC sp_trace_setstatus @TraceID, 1

	SET @FileName = @FileName + '.trc'

	-- Return how to Stop trace--
	-----------------------------
	PRINT 'sp_trace_setstatus '+ CAST(@TraceID as VARCHAR(2))+', 0'
	PRINT 'GO'
	PRINT 'sp_trace_setstatus '+ CAST(@TraceID as VARCHAR(2))+', 2'
	
END TRY
BEGIN CATCH

	DECLARE	@ErrorMessage		NVARCHAR(4000)
	DECLARE	@ErrorNumber		INT
	DECLARE	@ErrorSeverity		INT
	DECLARE	@ErrorState			INT
	DECLARE	@ErrorLine			INT
	DECLARE	@ErrorProcedure		NVARCHAR(1000)

	IF ERROR_NUMBER() IS NOT NULL
	BEGIN
		SET	@ErrorNumber	= ERROR_NUMBER()
		SET	@ErrorSeverity	= ERROR_SEVERITY()
		SET	@ErrorState		= ERROR_STATE()
		SET	@ErrorLine		= ERROR_LINE()
		SET	@ErrorProcedure	= ISNULL(ERROR_PROCEDURE(), '-')

		-- Write an error message and the original error information to the server log.
		SET @ErrorMessage = 'Error %d, Severity %d, State %d, Procedure %s, Line %d, Message: ' + ERROR_MESSAGE()
		RAISERROR (@ErrorMessage, 16, 1, @ErrorNumber, @ErrorSeverity, @ErrorState, @ErrorProcedure, @ErrorLine) WITH LOG
	END

END CATCH
GO

/* spTraceStart *******************************************************/
