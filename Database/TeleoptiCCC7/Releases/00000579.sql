  CREATE table #double(foreignid nvarchar(100) COLLATE database_default )

  -- doubles
  INSERT INTO #double
  select  ForeignId FROM ApplicationFunction f
  INNER JOIN ApplicationFunctionInRole fr ON f.Id = fr.ApplicationFunction
  where ForeignSource = 'Matrix'
  GROUP BY ForeignId, fr.ApplicationRole
   HAVING count(*) > 1

    CREATE table #minid(id uniqueidentifier, foreignid nvarchar(100) COLLATE database_default   )
            INSERT INTO #minid
            SELECT MIN(id), d.foreignid  FROM ApplicationFunction f
            INNER JOIN #double d ON f.ForeignId = d.foreignid
            GROUP BY d.foreignid

   --all id with those foreign
   CREATE table #tochange(id uniqueidentifier, foreignid nvarchar(100 ) COLLATE database_default , thenewid uniqueidentifier)
   INSERT INTO #tochange
   SELECT f.id, f.ForeignId, m.id FROM ApplicationFunction f
   INNER JOIN #minid m ON m.foreignid = f.ForeignId

  DELETE FROM ApplicationFunctionInRole 
  from ApplicationFunctionInRole a 
  INNER JOIN #tochange c
  ON a.ApplicationFunction = c.id
  WHERE id <> thenewid

  DELETE FROM ApplicationFunction
  where ForeignSource = 'Matrix'
  AND Id not in(SELECT ApplicationFunction FROM ApplicationFunctionInRole)
