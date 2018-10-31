DECLARE @start datetimeoffset
SELECT @start=getdate()
DECLARE @state_desc nvarchar(120)
DECLARE @dbid int
SELECT @dbid=database_id, @state_desc=state_desc FROM sys.databases 
WHERE name='$(DBNAME)' 
WHILE (@state_desc!='ONLINE') 
BEGIN 
        -- delay for another 10 seconds 
        WAITFOR DELAY '00:00:10.000'
    SELECT @dbid=database_id, @state_desc=state_desc FROM sys.databases 
    WHERE name='$(DBNAME)' 
END
SELECT 'Completed (in mins):',DATEDIFF(mi,@start,getdate())