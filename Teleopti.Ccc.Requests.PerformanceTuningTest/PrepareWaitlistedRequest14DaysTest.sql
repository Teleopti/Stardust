-- ta bort alla PersonAbsence d�r vi vill leka d� blir det l�ttare att f� godk�nt
declare @start datetime = '2016-04-03 00:00:00' 
declare @end datetime = '2016-04-20 00:00:00' 

declare @startRequest datetime = '2016-04-06 04:30:00' 
declare @endRequest datetime = '2016-04-06 08:00:00'
declare @startRequest2 datetime = '2016-04-06 09:00:00' 
declare @endRequest2 datetime = '2016-04-06 13:30:00'

delete 
from PersonAbsence
where [Minimum] between @start and @end or Maximum between @start and @end
DELETE FROM AbsenceRequest WHERE Request IN (SELECT Id from Request where StartDateTime between  @start  and @end)
delete from Request where StartDateTime between  @start  and @end and Id not in (select Request from ShiftTradeRequest)
delete from PersonRequest where id not in(select parent from Request )

SET @start = '2016-04-06'
-- trunka f�r att f� bort skr�p

truncate table [ReadModel].[SkillCombinationResource]
truncate table [ReadModel].[SkillCombination]
truncate table [ReadModel].[SkillCombinationResourceDelta]
truncate table [dbo].[JobStartTime]

-- ladda 100 (lite random ) personer p� ett bu - '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B'
-- ska ha person period och helst schema under perioden
-- skapa person request p� dom

create table #tempPerson ( id uniqueidentifier )

-- allow much understaffing so more gets approved
update Skill set SeriousUnderstaffing = -0.5, Understaffing = -0.5
update Skill set SeriousUnderstaffing = -0.4, Understaffing = -0.4
WHERE Id = '54ABB467-09C6-480F-B705-A3B701042B80' 

declare @PersonRequestId uniqueidentifier 
declare @RequestId uniqueidentifier 
declare @person_id uniqueidentifier

DECLARE @cnt INT = 0;
WHILE @cnt < 14
BEGIN
	insert into #tempPerson 
	select distinct 
	--top 50 
	p.id from person p
	inner join personperiod pp on pp.Parent = p.id
	inner join Team t on t.id = pp.team
	inner join [Site] s on t.Site = s.Id
	inner join PersonAssignment pa on pa.Person = p.Id
	inner join PersonSkill ps on pp.Id = ps.Parent
	where  DATEADD(d, @cnt, @start) BETWEEN pp.StartDate and pp.EndDate 
	and p.WorkflowControlSet = 'E97BC114-8939-4A70-AE37-A338010FFF19'
	and s.BusinessUnit = '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B'
	and pa.Date = DATEADD(d, @cnt, @start)
	and Skill = '54ABB467-09C6-480F-B705-A3B701042B80'
	and pa.ShiftCategory IS NOT NULL

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
	'2016-12-25', '2016-12-25',@person_id, 5, 'Performance test waitlist', 'of absence request', 0,  '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B', '', GETUTCDATE(),null

	select @RequestId = newid()
	INSERT INTO Request SELECT @RequestId, @PersonRequestId, DATEADD(d, @cnt, @startRequest), DATEADD(d, @cnt, @endRequest)
	INSERT INTO AbsenceRequest SELECT @RequestId, '3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F' -- SEMESTER

	select @PersonRequestId = newid()
	insert into PersonRequest select @PersonRequestId, 1, '3F0886AB-7B25-4E95-856A-0D726EDC2A67', '3F0886AB-7B25-4E95-856A-0D726EDC2A67', 
	'2016-12-25', '2016-12-25',@person_id, 5, 'Performance test waitlist', 'of absence request', 0,  '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B', '', GETUTCDATE(),null

	select @RequestId = newid()
	INSERT INTO Request SELECT @RequestId, @PersonRequestId, DATEADD(d, @cnt, @startRequest2), DATEADD(d, @cnt, @endRequest2)
	INSERT INTO AbsenceRequest SELECT @RequestId, '3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F' -- SEMESTER

		FETCH NEXT FROM request_cursor   
		INTO @person_id 
	END  
 
	CLOSE request_cursor;  
	DEALLOCATE request_cursor;
 
   SET @cnt = @cnt + 1;
   TRUNCATE TABLE #tempPerson
END;

drop table #tempPerson
/*
select * from  PersonRequest where CreatedOn = '2016-12-24' AND requestStatus = 4
select * from Request where Parent in(select id from PersonRequest where CreatedOn = '2016-12-24')

DELETE FROM AbsenceRequest WHERE Request IN (SELECT Id from Request where Parent in(select id from PersonRequest where CreatedOn = '2016-12-24'))
delete from Request where Parent in(select id from PersonRequest where CreatedOn = '2016-12-24')
DELETE FROM PersonRequest where CreatedOn = '2016-12-24'

//persons who will be denied
select Id From PersonRequest where CreatedOn = '2016-12-24' AND 
(Person = '32493FCB-0F34-4189-954C-A3DA00E7F828' OR
 Person = 'D59496AE-7957-4238-9CC6-A1410111B417' OR
 Person = '3789F038-A034-46BE-9E11-A1410113C474' OR
 Person = 'B8A547A6-FF73-4603-9229-A1410113C44E' OR
 Person = '435F77DD-88FD-4B5F-A50B-A3DA00E7F828' OR
 Person = '9E96BA4A-62F1-4153-A2D9-A2370109DB53' OR
 Person = '37166772-E0C0-4E8D-9E84-A20D00E94FE8' OR
 Person = 'CE4615F6-2559-48E6-9B0A-A1410113C453' OR
 Person = 'DCCDDEBA-CF95-45C1-8931-A1B700E17C23')

personRequest.Id.Value == new Guid( "F4B2B180-A729-43B1-A945-0CEE4D551A28") || personRequest.Id.Value == new Guid( "4E3F21FB-BC80-4B19-921D-42CD58568F8C") || personRequest.Id.Value == new Guid( "012CA641-F4ED-47D3-8EED-710148BB9D51") || personRequest.Id.Value == new Guid( "9AA91A10-4A32-4F03-A554-8CDA880FD147") || personRequest.Id.Value == new Guid( "DF4E7327-6C5A-4649-B55B-94CB40F443BF") || personRequest.Id.Value == new Guid( "D553E477-5A1E-4D50-8CA7-AD5F248970C9") || personRequest.Id.Value == new Guid( "79D18C88-A47F-44F8-8DB3-BE3087C24DA5") || personRequest.Id.Value == new Guid( "629CACDE-51FB-4A12-9917-C7D46EDBA1FD") 
*/