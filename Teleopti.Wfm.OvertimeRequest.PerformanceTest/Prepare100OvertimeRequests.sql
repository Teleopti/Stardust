declare @start datetime = '2016-01-16 00:00:00' 
declare @end datetime = '2016-01-17 00:00:00' 
declare @startRequest datetime = '2016-01-16 17:00:00' 
declare @endRequest datetime = '2016-01-16 19:00:00'

-- delete existing requests
DELETE FROM Overtimerequest WHERE Request IN (SELECT Id from Request where StartDateTime between  @start  and @end)
DELETE FROM AbsenceRequest WHERE Request IN (SELECT Id from Request where StartDateTime between  @start  and @end)
delete from Request where StartDateTime between  @start  and @end and Id not in (select Request from ShiftTradeRequest)
delete from PersonRequest where id not in(select parent from Request )

-- truncate data from other test runs
truncate table [ReadModel].[SkillCombinationResource]
truncate table [ReadModel].[SkillCombination]
truncate table [ReadModel].[SkillCombinationResourceDelta]
truncate table [dbo].[JobStartTime]

-- delete existing OvertimeRequestOpenPeriod
DELETE FROM OvertimeRequestOpenPeriod WHERE Parent = 'E97BC114-8939-4A70-AE37-A338010FFF19'

INSERT INTO [dbo].[OvertimeRequestOpenPeriod]
([Id],[PeriodType],[OrderIndex],[Parent],[DaysMinimum],[DaysMaximum],[Minimum],[Maximum],[AutoGrantType],[EnableWorkRuleValidation],[WorkRuleValidationHandleType])
VALUES
(newid(),'DatePeriod',0,'E97BC114-8939-4A70-AE37-A338010FFF19',NULL,NULL,'2016-01-01 00:00:00.000','2017-01-01 00:00:00.000',1,1,0)

-- load 100 persons on bu '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B'
-- should have personPeriod and schedule during period
select distinct top 100 p.* into #tempPerson 
from person p
inner join personperiod pp on pp.Parent = p.id
inner join Team t on t.id = pp.team
inner join [Site] s on t.Site = s.Id
inner join PersonAssignment pa
on pa.Person = p.Id
where pp.StartDate < @end and pp.EndDate > @start
and p.WorkflowControlSet = 'E97BC114-8939-4A70-AE37-A338010FFF19'
and s.BusinessUnit = '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B'
and pa.Date between @start and @end


-- allow little understaffing so more gets approved
update Skill set SeriousUnderstaffing = 100, Understaffing = 110


declare @PersonRequestId uniqueidentifier 
declare @RequestId uniqueidentifier 
declare @person_id uniqueidentifier

DECLARE request_cursor CURSOR FOR   
SELECT Id  
FROM #tempPerson  
OPEN request_cursor  
  
FETCH NEXT FROM request_cursor   
INTO @person_id
  
WHILE @@FETCH_STATUS = 0  
BEGIN  
    select @PersonRequestId = newid()
insert into PersonRequest select @PersonRequestId, 1, '3F0886AB-7B25-4E95-856A-0D726EDC2A67', '3F0886AB-7B25-4E95-856A-0D726EDC2A67', 
'2016-12-31', '2016-12-31',@person_id, 0, 'Performance test', 'of overtime request', 0,  '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B', '', GETUTCDATE(),null

select @RequestId = newid()
INSERT INTO Request SELECT @RequestId, @PersonRequestId, @startRequest, @endRequest

INSERT INTO OvertimeRequest SELECT @RequestId, 'E0D49526-3CCB-4A17-B7D2-A142010BBDB4' -- �vertid Betald

    FETCH NEXT FROM request_cursor   
    INTO @person_id 
END  
 
CLOSE request_cursor;  
DEALLOCATE request_cursor;


drop table #tempPerson
