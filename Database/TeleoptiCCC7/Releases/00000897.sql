--> moved to Programmability\02Functions
IF EXISTS (SELECT * FROM sys.objects WHERE [object_id] = OBJECT_ID(N'ReadModelAgentState')
               AND [type] = 'TR')
BEGIN
      DROP TRIGGER ReadModelAgentState;
END;
GO