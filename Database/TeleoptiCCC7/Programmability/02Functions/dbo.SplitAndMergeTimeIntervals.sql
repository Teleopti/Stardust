IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitAndMergeTimeInterval]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[SplitAndMergeTimeInterval]
GO

/*
--intersecting and/or overlapping intervals:
SELECT * FROM dbo.SplitAndMergeTimeInterval('2014-08-08 10:00;2014-08-08 12:00,2014-08-08 12:00;2014-08-08 14:00,2014-08-08 13:00;2014-08-08 14:33')

--seprated by one minute:
SELECT * FROM dbo.SplitAndMergeTimeInterval('2014-08-08 10:00;2014-08-08 12:00,2014-08-08 12:01;2014-08-08 14:00,2014-08-08 14:15;2014-08-08 14:30')
*/

CREATE FUNCTION [dbo].[SplitAndMergeTimeInterval]
(@timeIntervals_string varchar(max))
RETURNS @mergedIntervalList TABLE (
startTime smalldatetime NOT NULL,
endTime smalldatetime NOT NULL
)

AS
BEGIN 

	DECLARE @IntervalList table
	(
		startTimeStart smalldatetime,
		startTimeEnd smalldatetime,
		overlap smallint
	)

	declare @mindate smalldatetime
	declare @yesterday smalldatetime
	declare @previousDate smalldatetime
	declare @previousEnd smalldatetime
	declare @overlap smallint

	SET @mindate = '2000-01-01 00:00'
	SET @yesterday = DATEDIFF(DD, -1, GETDATE())

	DECLARE @pos int
	DECLARE @string varchar(50)
	DECLARE @insert_text varchar(100)
	-- Exit if an empty string is given 
	IF @timeIntervals_string = '' BEGIN
		RETURN 
	END 

	INSERT INTO @IntervalList(startTimeStart, startTimeEnd)
	SELECT
		CAST(LEFT(string,CHARINDEX(';',string)-1) as smalldatetime),
		CAST(RIGHT(string,CHARINDEX(';',string)-1) as smalldatetime)
	FROM dbo.SplitStringString(@timeIntervals_string)

	SELECT @previousDate=@mindate, @previousEnd=@yesterday,@overlap =0
	UPDATE @IntervalList
	SET @overlap=overlap=
		CASE
			WHEN @previousEnd<startTimeStart THEN @overlap+1
			ELSE @overlap
		END,
	@previousEnd = startTimeEnd

	INSERT INTO @mergedIntervalList(startTime,endTime)
	SELECT
		min(startTimeStart),
		max(startTimeEnd)
	FROM @IntervalList
	GROUP BY overlap

	RETURN
END
