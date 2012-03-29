----------------  
--Name: AndersF
--Date: 2012-03-29
--Desc: #18790 - Performance: etl fact report permissions is performing bad
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[permission_report]') AND name = N'IX_Permission_Report_BusinessUnitId')
CREATE NONCLUSTERED INDEX [IX_Permission_Report_BusinessUnitId]
ON [mart].[permission_report] ([business_unit_id])
INCLUDE ([person_code],[team_id],[my_own],[ReportId])
GO
