-- ta bort alla PersonAbsence där vi vill leka då blir det lättare att få godkänt
declare @start datetime = '2016-02-29 23:00:00' 
declare @end datetime = '2016-03-02 23:00:00' 


--select * 
delete 
from PersonAbsence
where [Minimum] between @start and @end or Maximum between @start and @end

DELETE FROM AbsenceRequest WHERE Request IN (SELECT Id from Request where StartDateTime between  @start  and @end)
delete from Request where StartDateTime between  @start  and @end and Id not in (select Request from ShiftTradeRequest)
delete from PersonRequest where id not in(select parent from Request )

-- ladda 200 (lite random ) personer på ett bu - '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B'
-- ska ha person period och helst schema under perioden
-- skapa person request på dom
select distinct top 200 p.* into #tempPerson 
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

-- allow much understaffing so more gets approved
update Skill set SeriousUnderstaffing = -1, Understaffing = -1

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
'2016-12-24', '2016-12-24',@person_id, 0, 'Performance test', 'of absence request', 0,  '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B', '', GETUTCDATE(),null

select @RequestId = newid()
INSERT INTO Request SELECT @RequestId, @PersonRequestId, @start, @end

INSERT INTO AbsenceRequest SELECT @RequestId, '3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F' -- SEMESTER

    FETCH NEXT FROM request_cursor   
    INTO @person_id 
END   
CLOSE request_cursor;  
DEALLOCATE request_cursor;

drop table #tempPerson
/*
select * from  PersonRequest where CreatedOn = '2016-12-24'
select * from Request where Parent in(select id from PersonRequest where CreatedOn = '2016-12-24')

DELETE FROM AbsenceRequest WHERE Request IN (SELECT Id from Request where Parent in(select id from PersonRequest where CreatedOn = '2016-12-24'))
delete from Request where Parent in(select id from PersonRequest where CreatedOn = '2016-12-24')
DELETE FROM PersonRequest where CreatedOn = '2016-12-24'

*/