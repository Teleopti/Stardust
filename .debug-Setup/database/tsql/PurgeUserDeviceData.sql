IF  EXISTS (SELECT * FROM $(DATABASEAPP).sys.objects WHERE object_id = OBJECT_ID(N'$(DATABASEAPP).[dbo].[UserDevice]') AND type = N'U')
TRUNCATE TABLE $(DATABASEAPP).[dbo].[UserDevice]
GO