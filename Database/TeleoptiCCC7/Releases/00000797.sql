----------------  
--Name: Jonas Nordh
--Date: 2017-01-25
--Desc: Cleaning data and adding unique constraint for SkillDataPeriod to avoid duplicate records.
---------------- 

WITH cte as(
  SELECT ROW_NUMBER() OVER (PARTITION BY Parent, StartDateTime
                            ORDER BY Id) RN
  FROM   dbo.SkillDataPeriod
  )
DELETE FROM cte WHERE RN>1

ALTER TABLE dbo.SkillDataPeriod ADD CONSTRAINT UQ_SkillDataPeriod_Parent_StartDateTime UNIQUE NONCLUSTERED 
(
	[Parent] ASC,
	[StartDateTime] ASC
) ON [PRIMARY]

GO