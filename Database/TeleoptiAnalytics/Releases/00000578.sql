----------------  
--Name: Jonas Nordh
--Date: 2017-02-28
--Desc: Cleaning data and adding unique constraint for mart.dim_shift_length to avoid duplicate records.
---------------- 

WITH cte as(
  SELECT ROW_NUMBER() OVER (PARTITION BY shift_length_m
                            ORDER BY shift_length_id) RN
  FROM   mart.dim_shift_length
  )
DELETE FROM cte WHERE RN>1

ALTER TABLE mart.dim_shift_length ADD CONSTRAINT UQ_dim_shift_length UNIQUE NONCLUSTERED 
(
	shift_length_m ASC
) ON [PRIMARY]