IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[weeknumber]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[weeknumber]
GO




CREATE FUNCTION [mart].[weeknumber] (@start_week int, @DATE datetime)
	RETURNS int
	AS
	BEGIN
	/*
	Created by Micke DeigÕrd Teleopti AB
	
	Returns an int containing weeknumber for specifyed date.
	Example: 20050101 will return:
	53 (if first four day week is used) or
	1 (if 1st jan is used)
	
	*/
		declare @r varchar(6)
	
		if @start_week = 1 -- 1st jan
		begin
			select @r = convert(varchar(4),DATEPART(yy,@DATE))+right(convert(varchar(3),DATEPART(wk,@DATE)+100),2)
		end
		else if @start_week = 2 --First four day week
		begin
			--Requires monday as 1st day of week
			DECLARE @week int, @year int
	
			SET @week= DATEPART(wk,@DATE)+1
			-DATEPART(wk,CAST(DATEPART(yy,@DATE) as CHAR(4))+right(convert(char(5),10104),4))
			SET @year = DATEPART(yy,@DATE)
	
			--Special case: Dec 29-31 may belong to the next year
			if ((DATEPART(mm,@DATE)=12) AND 
			((DATEPART(dd,@DATE)-DATEPART(dw,@DATE))>= 28))
			begin
				set @week=1
				select @year=@year+1
			end
	
			--Special cases: Jan 1-3 may belong to the previous year
			if (@week=0)
			begin
				set @r=mart.yearweek(@start_week,CAST(DATEPART(yy,@DATE)-1 AS CHAR(4))+convert(char(2),12)+ CAST(24+DATEPART(DAY,@DATE) AS CHAR(2)))
				set @week = convert(int,right(@r,2))+1
				set @year = convert(int,left(@r,4))
			end
	
			SET @r = convert(varchar(4),@year)+right(convert(varchar(4),@week+100),2)
		end
	
		return(@week)
	END





GO

