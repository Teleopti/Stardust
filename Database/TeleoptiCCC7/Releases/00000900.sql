IF EXISTS (SELECT * FROM sys.objects WHERE [object_id] = OBJECT_ID(N'[ReadModel].[ReadModelAgentState]')
               AND [type] = 'TR')
BEGIN
      DROP TRIGGER [ReadModel].[ReadModelAgentState];
END;
GO