SET NOCOUNT ON
SELECT * FROM PerfTest_TeleoptiCCC7.dbo.SkillDataPeriod
GO
SELECT top 10000 * FROM fact_forecast_workload
SELECT top 10000 * FROM fact_kpi_targets_team
SELECT top 10000 * FROM fact_queue
DBCC FREEPROCCACHE
GO
DBCC DROPCLEANBUFFERS
GO