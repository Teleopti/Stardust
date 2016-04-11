CREATE TABLE [mart].[permission_report_execution](
	[person_code] [uniqueidentifier] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_permission_report_execution] PRIMARY KEY CLUSTERED ([person_code] ASC)
);

GO