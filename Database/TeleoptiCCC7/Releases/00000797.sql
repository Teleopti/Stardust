----------------  
--Name: Jonas Nordh
--Date: 2017-01-25
--Desc: Cleaning data and adding unique constraint for SkillDataPeriod to avoid duplicate records.
---------------- 

delete SkillDataPeriod
from SkillDataPeriod sdp1
inner join	(select Parent, StartDateTime, max(Id) IdMax, count(*) c 
			from SkillDataPeriod
			group by Parent, StartDateTime
			having count(*) > 1
			) as sdp2
			on sdp1.Id = sdp2.IdMax

ALTER TABLE dbo.SkillDataPeriod ADD CONSTRAINT UQ_SkillDataPeriod_Parent_StartDateTime UNIQUE NONCLUSTERED 
(
	[Parent] ASC,
	[StartDateTime] ASC
) ON [PRIMARY]

GO