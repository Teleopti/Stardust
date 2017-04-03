/****** Object:  StoredProcedure [mart].[raptor_stat_agent_queue]    Script Date: 09/01/2009 15:17:33 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_stat_agent_queue]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_stat_agent_queue]
GO

/****** Object:  StoredProcedure [mart].[raptor_stat_agent_queue]    Script Date: 09/01/2009 15:17:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:            AF
-- Create date: 2008-09-01
-- Update Date: 
-- Description:       Used by SDK and MyTime MyReport (the third panel)
-- =============================================

CREATE PROCEDURE [mart].[raptor_stat_agent_queue]
@date_from datetime,
@date_to datetime,
@person_code uniqueidentifier,
@time_zone_code nvarchar(200)
AS
SET NOCOUNT ON

DECLARE @time_zone_id int

SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone WHERE time_zone_code = @time_zone_code

SELECT     tricky.person_code,
                      q.queue_description,
                      sum(answered_calls) answered_calls,
                      CASE WHEN sum(answered_calls)=0 THEN 0 ELSE sum(talk_time_s)/sum(answered_calls) END average_talk_time,
                      CASE WHEN sum(answered_calls)=0 THEN 0 ELSE sum(after_call_work_time_s)/sum(answered_calls) END average_after_call_work,
                      CASE WHEN sum(answered_calls)=0 THEN 0 ELSE (sum(after_call_work_time_s)+sum(talk_time_s))/sum(answered_calls) END average_handling_time
FROM mart.fact_agent_queue fa
INNER JOIN mart.dim_queue q
           ON fa.queue_id = q.queue_id
INNER JOIN mart.bridge_time_zone b
           ON         fa.interval_id= b.interval_id
           AND fa.date_id= b.date_id
           AND b.time_zone_id = @time_zone_id
INNER JOIN mart.dim_date d 
           ON b.local_date_id = d.date_id
INNER JOIN (SELECT DISTINCT ba.acd_login_id, p.person_code
                                 FROM mart.bridge_acd_login_person ba
                                 INNER JOIN mart.dim_person p  WITH (NOLOCK)
                                            on p.person_id=ba.person_id
                                            AND (@date_from	between p.valid_from_date AND p.valid_to_date OR  @date_to	between p.valid_from_date AND p.valid_to_date)
                                            AND p.person_code = @person_code) tricky
                                 ON fa.acd_login_id = tricky.acd_login_id
WHERE d.date_date BETWEEN @date_from AND @date_to
GROUP BY tricky.person_code,q.queue_description


GO


