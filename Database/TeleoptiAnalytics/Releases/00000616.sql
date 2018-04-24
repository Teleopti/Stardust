-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-23
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

ALTER TABLE dbo.AdvancedLoggingService ADD CONSTRAINT
	PK_AdvancedLoggingService PRIMARY KEY NONCLUSTERED 
	(
		LogDate
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.AdvancedLoggingService SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------

ALTER TABLE stage.stg_queue ADD CONSTRAINT
	PK_stg_queue PRIMARY KEY NONCLUSTERED 
	(
		date,
		interval,
		queue_name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE stage.stg_queue SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
DROP TABLE stage.stg_permission_report
GO

CREATE TABLE stage.stg_permission_report
	(
	id int NOT NULL IDENTITY (1, 1),
	person_code uniqueidentifier NULL,
	ReportId uniqueidentifier NULL,
	team_id uniqueidentifier NULL,
	my_own bit NULL,
	business_unit_code uniqueidentifier NOT NULL,
	business_unit_name nvarchar(50) NOT NULL,
	datasource_id smallint NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL,
	datasource_update_date smalldatetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE stage.stg_permission_report SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE stage.stg_permission_report ADD CONSTRAINT
	DF_stg_permission_datasource_id DEFAULT ((1)) FOR datasource_id
GO
ALTER TABLE stage.stg_permission_report ADD CONSTRAINT
	DF_stg_permission_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE stage.stg_permission_report ADD CONSTRAINT
	DF_stg_permission_update_date DEFAULT (getdate()) FOR update_date
GO
ALTER TABLE stage.stg_permission_report ADD CONSTRAINT
	DF_stg_permission_report_datasource_update_date DEFAULT (getdate()) FOR datasource_update_date
GO
ALTER TABLE stage.stg_permission_report ADD CONSTRAINT
	PK_stg_permission_report PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
