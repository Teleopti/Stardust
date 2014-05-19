IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sp_dummy]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].sp_dummy
GO

create proc mart.sp_dummy
as

--Make Insert NoCount
SET NOCOUNT ON
--insert into mart.fact_schedule
SELECT * from mart.fact_schedule_old

--Start counting rows as we delete from mart.fact_schedule_old
--This is how we detect "done" from C# when rows effected = 0
SET NOCOUNT OFF
delete top (0) from mart.fact_schedule_old
