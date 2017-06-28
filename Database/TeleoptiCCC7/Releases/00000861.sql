ALTER TABLE OvertimeRequest DROP COLUMN OvertimeType
GO
ALTER TABLE OvertimeRequest ADD MultiplicatorDefinitionSet UNIQUEIDENTIFIER NOT NULL,
CONSTRAINT FK_OvertimeRequest_MultiplicatorDefinitionSet
FOREIGN KEY (MultiplicatorDefinitionSet) REFERENCES MultiplicatorDefinitionSet(Id)
GO