IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_load_agent_count]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_load_agent_count]
GO

-- =============================================
-- Author:		Unknown
-- Create date: 2008-xx-xx
-- Update date: 20090803 
-- 20090211 Added new mart schema KJ
-- Description:	Return the number of active agents grouped by skill and date
-- 20090427 Changed dateformat on mindate
-- 20090803 Removed some groupings /ZT
-- 20100326 Changed implementation as part of bug 9616, active agents now calculated from time instead of count
-- 20100326 Fix to handle an empty datamart DJ
-- 20111206 Removed the interval length calculation as that wasn't used anymore. Added with nolock for query.
-- 20121123 Bug #21451 Speed up intraday
-- =============================================
CREATE PROCEDURE [mart].[raptor_load_agent_count]
	(
	@skill		uniqueidentifier,
	@start_date datetime,
	@end_date	datetime
	)
AS
 
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #temp(
		[date_id] [int] NOT NULL,
		[interval_id] [smallint] NOT NULL,
		[acd_login_id] [int] NOT NULL,
		[ready_time_s] decimal(20,2) NULL
	)

	insert into #temp
	select f.date_id, f.interval_id, f.acd_login_id, f.ready_time_s
	from mart.fact_agent f WITH (NOLOCK) 
	inner join mart.dim_date d
		on d.date_id=f.date_id
	where d.date_date between convert (varchar (10),@start_date,120) and convert (varchar(10),@end_date,120)
 
 
--Create mindate
DECLARE @mindate as smalldatetime
SELECT @mindate=CAST('19000101' as smalldatetime)

SELECT
	DATEADD(mi, DATEDIFF(mi,@mindate,mart.dim_interval.interval_start), mart.dim_date.date_date) as [Interval],
	COUNT (DISTINCT #temp.acd_login_id) as [ActiveAgents] --Heads!
	FROM #temp WITH (NOLOCK)
	INNER JOIN mart.bridge_acd_login_person WITH (NOLOCK)
		ON mart.bridge_acd_login_person.acd_login_id=#temp.acd_login_id
	INNER JOIN mart.dim_person WITH (NOLOCK)
		ON mart.dim_person.person_id=mart.bridge_acd_login_person.person_id
	INNER JOIN mart.bridge_skillset_skill WITH (NOLOCK)
		ON mart.dim_person.skillset_id=mart.bridge_skillset_skill.skillset_id
	INNER JOIN mart.dim_skill WITH (NOLOCK)
		ON mart.bridge_skillset_skill.skill_id = mart.dim_skill.skill_id
	INNER JOIN mart.dim_date WITH (NOLOCK)
		ON mart.dim_date.date_id=#temp.date_id
	INNER JOIN mart.dim_interval WITH (NOLOCK)
		ON mart.dim_interval.interval_id=#temp.interval_id
	 
	WHERE
			mart.dim_person.valid_from_date<@end_date
	AND		mart.dim_person.valid_to_date>@start_date
	AND		mart.dim_skill.skill_code=@skill
	AND		DATEADD(mi, DATEDIFF(mi,@mindate,mart.dim_interval.interval_start), mart.dim_date.date_date) BETWEEN @start_date AND @end_date
	AND		isnull(#temp.ready_time_s,0) > 0
	GROUP BY mart.dim_interval.interval_start, mart.dim_date.date_date
	ORDER BY Interval
END
GO
