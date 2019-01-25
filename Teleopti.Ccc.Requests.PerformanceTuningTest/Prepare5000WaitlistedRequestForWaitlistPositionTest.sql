DECLARE @start DATETIME = '2016-04-03 00:00:00' 
DECLARE @end DATETIME = '2016-04-09 00:00:00' 
DECLARE @startRequest DATETIME = '2016-04-06 8:00:00' 
DECLARE @endRequest DATETIME = '2016-04-06 17:00:00' 
DECLARE @waitlistedStatus INT = 5 
DECLARE @pendingStatus INT = 0 
DECLARE @businessUnitId uniqueidentifier = '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B'
DECLARE @absenceTypeId uniqueidentifier = '3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F'
DECLARE @systemUserId uniqueidentifier = '3F0886AB-7B25-4E95-856A-0D726EDC2A67'

DELETE FROM personabsence 
WHERE  [minimum] BETWEEN @start AND @end 
		OR maximum BETWEEN @start AND @end 

DELETE FROM absencerequest 
WHERE  request IN (SELECT id 
					FROM   request 
					WHERE  startdatetime BETWEEN @start AND @end) 

DELETE FROM request 
WHERE  startdatetime BETWEEN @start AND @end 
		AND id NOT IN (SELECT request 
						FROM   shifttraderequest) 

DELETE FROM personrequest 
WHERE  id NOT IN(SELECT parent 
					FROM   request) 

SELECT DISTINCT TOP 2500 p.* 
INTO   #tempperson 
FROM   person p 
		INNER JOIN personperiod pp 
				ON pp.parent = p.id 
		INNER JOIN team t 
				ON t.id = pp.team 
		INNER JOIN [site] s 
				ON t.site = s.id 
		INNER JOIN WorkflowControlSet wcs 
				ON wcs.Id = p.WorkflowControlSet
WHERE  pp.startdate < @end 
		AND pp.enddate > @start 
		AND s.businessunit = @businessUnitId 
		AND wcs.IsDeleted = 0
		
DECLARE @PersonRequestId UNIQUEIDENTIFIER 
DECLARE @RequestId UNIQUEIDENTIFIER 
DECLARE @person_id UNIQUEIDENTIFIER 
DECLARE request_cursor CURSOR FOR 
	SELECT id 
	FROM   #tempperson 

OPEN request_cursor 

FETCH next FROM request_cursor INTO @person_id 

DECLARE @Count INT = 0;
WHILE @@FETCH_STATUS = 0 
	BEGIN 
		-- waitlisted request begin
		SELECT @PersonRequestId = Newid() 

		SET @Count = @Count +15;

		INSERT INTO personrequest 
		SELECT @PersonRequestId, 
				1, 
				@systemUserId, 
				@systemUserId, 
				DATEADD(second, @COUNT,GETUTCDATE()), 
				'2017-10-24', 
				@person_id, 
				@waitlistedStatus, 
				'Performance test waitlist position', 
				'waitlisted absence request', 
				0, 
				@businessUnitId, 
				'', 
				Getutcdate(), 
				NULL 

		SELECT @RequestId = Newid() 

		INSERT INTO request 
		SELECT @RequestId, 
				@PersonRequestId, 
				@startRequest, 
				@endRequest 

		INSERT INTO absencerequest 
		SELECT @RequestId, 
				@absenceTypeId 
		-- waitlisted request end

		-- pending request begin
		SELECT @PersonRequestId = Newid()

		SET @Count = @Count +15;

		INSERT INTO personrequest 
		SELECT @PersonRequestId, 
				1, 
				@systemUserId, 
				@systemUserId, 
				DATEADD(second, @COUNT,GETUTCDATE()), 
				'2017-10-24', 
				@person_id, 
				@pendingStatus, 
				'Performance test waitlist position', 
				'pending absence request', 
				0, 
				@businessUnitId, 
				'', 
				Getutcdate(), 
				NULL 

		SELECT @RequestId = Newid() 

		INSERT INTO request 
		SELECT @RequestId, 
				@PersonRequestId, 
				@startRequest, 
				@endRequest 

		INSERT INTO absencerequest 
		SELECT @RequestId, 
				@absenceTypeId
		-- pending request end

	FETCH next FROM request_cursor INTO @person_id 
	END 

CLOSE request_cursor; 

DEALLOCATE request_cursor; 

DROP TABLE #tempperson 