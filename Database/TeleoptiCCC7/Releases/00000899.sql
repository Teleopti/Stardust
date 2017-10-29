IF EXISTS (SELECT * FROM sys.objects WHERE [object_id] = OBJECT_ID(N'[ReadModel].[tr_AgentState]')
               AND [type] = 'TR')
BEGIN
      DROP TRIGGER [ReadModel].[tr_AgentState];
END;
GO