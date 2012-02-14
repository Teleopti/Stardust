/* 
Trunk initiated: 
2010-10-04 
10:50
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Jonas N
--Date: 2010-10-19  
--Desc: Add a column to mart.pm_user to keep track of windows and application logons
----------------  
ALTER TABLE mart.pm_user
	DROP CONSTRAINT DF_pm_user_insert_date
GO
ALTER TABLE mart.pm_user
	DROP CONSTRAINT DF_pm_user_update_date
GO
CREATE TABLE mart.Tmp_pm_user
	(
	user_name nvarchar(256) NOT NULL,
	is_windows_logon bit NOT NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE mart.Tmp_pm_user ADD CONSTRAINT
	DF_pm_user_is_windows_logon DEFAULT 1 FOR is_windows_logon
GO
ALTER TABLE mart.Tmp_pm_user ADD CONSTRAINT
	DF_pm_user_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_pm_user ADD CONSTRAINT
	DF_pm_user_update_date DEFAULT (getdate()) FOR update_date
GO
IF EXISTS(SELECT * FROM mart.pm_user)
	 EXEC('INSERT INTO mart.Tmp_pm_user (user_name, insert_date, update_date)
		SELECT user_name, insert_date, update_date FROM mart.pm_user WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE mart.pm_user
GO
EXECUTE sp_rename N'mart.Tmp_pm_user', N'pm_user', 'OBJECT' 
GO
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (299,'7.1.299') 
