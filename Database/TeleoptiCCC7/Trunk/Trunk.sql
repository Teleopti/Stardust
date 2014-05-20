--Name: Chundan Xu, Mingdi Wu
--Date: 2014-05-20
--Desc: Add minTimePerWeek to work flow control set table
----------------  

ALTER TABLE dbo.WorkFlowControlset
ADD MinTimePerWeek bigint null;