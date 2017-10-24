DECLARE @start DATETIME = '2016-04-03 00:00:00' 
DECLARE @end DATETIME = '2016-04-09 00:00:00' 
DECLARE @startRequest DATETIME = '2016-04-06 8:00:00' 
DECLARE @endRequest DATETIME = '2016-04-06 17:00:00' 
DECLARE @waitlistedStatus INT = 5 
DECLARE @businessUnitId uniqueidentifier = '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B' 

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
WHERE  pp.startdate < @end 
	   AND pp.enddate > @start 
	   AND s.businessunit = @businessUnitId 

--double people 
INSERT INTO #tempperson 
SELECT * 
FROM   #tempperson 

DECLARE @PersonRequestId UNIQUEIDENTIFIER 
DECLARE @RequestId UNIQUEIDENTIFIER 
DECLARE @person_id UNIQUEIDENTIFIER 
DECLARE request_cursor CURSOR FOR 
  SELECT id 
  FROM   #tempperson 

OPEN request_cursor 

FETCH next FROM request_cursor INTO @person_id 

WHILE @@FETCH_STATUS = 0 
  BEGIN 
	  SELECT @PersonRequestId = Newid() 

	  INSERT INTO personrequest 
	  SELECT @PersonRequestId, 
			 1, 
			 '3F0886AB-7B25-4E95-856A-0D726EDC2A67', 
			 '3F0886AB-7B25-4E95-856A-0D726EDC2A67', 
			 '2017-10-24', 
			 '2017-10-24', 
			 @person_id, 
			 @waitlistedStatus, 
			 'Performance test waitlist position', 
			 'full day absence request', 
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
			 '3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F' 

	FETCH next FROM request_cursor INTO @person_id 
  END 

CLOSE request_cursor; 

DEALLOCATE request_cursor; 

DROP TABLE #tempperson 