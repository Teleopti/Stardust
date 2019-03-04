IF NOT EXISTS (SELECT * FROM [PurgeSetting] WHERE [Key]='DaysToKeepLoginsAfterTerminalDate')
INSERT INTO [PurgeSetting]
VALUES ('DaysToKeepLoginsAfterTerminalDate', 7)