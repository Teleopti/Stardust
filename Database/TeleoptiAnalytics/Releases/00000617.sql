-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-23
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

DROP TABLE stage.stg_acd_login_person
GO

CREATE TABLE stage.stg_acd_login_person
	(
	id int NOT NULL IDENTITY (1, 1),
	acd_login_code nvarchar(65) NULL,
	person_code uniqueidentifier NULL,
	start_date smalldatetime NULL,
	end_date smalldatetime NULL,
	person_period_code uniqueidentifier NULL,
	log_object_datasource_id int NULL,
	log_object_name nvarchar(50) NULL,
	datasource_id smallint NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL,
	datasource_update_date smalldatetime NULL
	)
GO
ALTER TABLE stage.stg_acd_login_person SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE stage.stg_acd_login_person ADD CONSTRAINT
	DF_acd_login_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE stage.stg_acd_login_person ADD CONSTRAINT
	DF_acd_login_update_date DEFAULT (getdate()) FOR update_date
GO
ALTER TABLE stage.stg_acd_login_person ADD CONSTRAINT
	DF_acd_login_datasource_update_date DEFAULT (getdate()) FOR datasource_update_date
GO
ALTER TABLE stage.stg_acd_login_person ADD CONSTRAINT
	PK_stg_acd_login_person PRIMARY KEY NONCLUSTERED 
	(
		id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX CIX_stg_acd_login_person ON stage.stg_acd_login_person
	(
		acd_login_code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON STAGE
GO



-----------------------------------------------------------
DROP TABLE stage.stg_overtime
GO
CREATE TABLE stage.stg_overtime
	(
	id int NOT NULL IDENTITY (1, 1),
	overtime_code uniqueidentifier NULL,
	overtime_name nvarchar(250) NULL,
	business_unit_code uniqueidentifier NOT NULL,
	business_unit_name nvarchar(50) NOT NULL,
	datasource_id smallint NOT NULL,
	insert_date smalldatetime NOT NULL,
	update_date smalldatetime NOT NULL,
	datasource_update_date smalldatetime NULL,
	is_deleted bit NOT NULL
	)
GO
ALTER TABLE stage.stg_overtime SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE stage.stg_overtime ADD CONSTRAINT
	DF_stg_multiplicator_definition_set_datasource_id DEFAULT ((1)) FOR datasource_id
GO
ALTER TABLE stage.stg_overtime ADD CONSTRAINT
	DF_stg_multiplicator_definition_set_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE stage.stg_overtime ADD CONSTRAINT
	DF_stg_multiplicator_definition_set_update_date DEFAULT (getdate()) FOR update_date
GO
ALTER TABLE stage.stg_overtime ADD CONSTRAINT
	DF_stg_multiplicator_definition_set_is_deleted DEFAULT ((0)) FOR is_deleted
GO
ALTER TABLE stage.stg_overtime ADD CONSTRAINT
	PK_stg_overtime PRIMARY KEY NONCLUSTERED 
	(
		id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX CIX_stg_overtime ON stage.stg_overtime
	(
		overtime_code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON STAGE
GO
