-- #43300
-- Find duplicate rows to be removed
create table #remove ([workload_id] int);

with duplicates as
(
select workload_id,
	ROW_NUMBER() over (partition by	workload_code order by workload_id asc) RowNumber --keep first preference by UTC interval
from mart.dim_workload
)
insert into #remove
select workload_id from duplicates where RowNumber > 1

--delete x 3
DELETE b
FROM mart.bridge_queue_workload b
INNER JOIN #remove r ON b.workload_id = r.workload_id

DELETE f
FROM mart.fact_forecast_workload f
INNER JOIN #remove r ON f.workload_id = r.workload_id

DELETE d
FROM mart.dim_workload d
INNER JOIN #remove r ON d.workload_id = r.workload_id

DROP TABLE #remove

--Already added with customer, use IF EXISTS
IF NOT EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='UC_workload_code')
BEGIN
	ALTER TABLE mart.dim_workload ADD CONSTRAINT UC_workload_code UNIQUE (workload_code);
END
GO


