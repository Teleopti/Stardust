-- Fix wrong data for [mart].[bridge_queue_workload]
-- Refer to bug #46863: Agent Queue stats not seen in the report for queues which were connected to some test workload and later deleted

DELETE bqw
  FROM mart.bridge_queue_workload bqw
 INNER JOIN mart.dim_workload dw ON bqw.workload_id = dw.workload_id
 WHERE dw.is_deleted = 1
    OR (bqw.queue_id != -1 AND bqw.workload_id = -1)

INSERT INTO mart.bridge_queue_workload (
       queue_id
     , workload_id
     , skill_id
     , business_unit_id
     , datasource_id
     , insert_date
     , update_date
     , datasource_update_date
)
SELECT queue_id               = dq.queue_id
     , workload_id            = - 1
     , skill_id               = - 1
     , business_unit_id       = - 1
     , datasource_id          = - 1
     , insert_date            = GETDATE()
     , update_date            = GETDATE()
     , datasource_update_date = ISNULL(dq.datasource_update_date, CAST('20591231' as smalldatetime))
  FROM mart.dim_queue dq
 WHERE queue_id NOT IN (SELECT queue_id FROM mart.bridge_queue_workload)
