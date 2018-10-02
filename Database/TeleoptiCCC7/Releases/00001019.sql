DECLARE 
   @ConstraintName nvarchar(200)
BEGIN
   SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS 
         WHERE 
		     PARENT_OBJECT_ID = OBJECT_ID('Stardust.WorkerNode') 
			 AND PARENT_COLUMN_ID = 
			     (SELECT column_id FROM sys.columns 
				   WHERE NAME = N'PingResult' 
				      AND object_id = OBJECT_ID(N'Stardust.WorkerNode'))

     if(@ConstraintName IS NOT NULL)
	 BEGIN
		EXEC('ALTER TABLE Stardust.WorkerNode DROP CONSTRAINT ' + @ConstraintName)

		ALTER TABLE Stardust.WorkerNode
		DROP COLUMN PingResult
	 END
END