--==========
-- Try blindly fix deadlock frenzy in database:
-- #40026: Deadlock on etl_bridge_group_page_person_delete_removed after user has applied changes in People
--==========
--new table
IF (SELECT ServerProperty('EngineEdition')) = 5
BEGIN
	CREATE TABLE [mart].[bridge_group_page_person_new](
		[group_page_id] [int] NOT NULL,
		[person_id] [int] NOT NULL,
		[datasource_id] [int] NOT NULL,
		[insert_date] [smalldatetime] NULL,
	 CONSTRAINT [PK_bridge_group_page_person_new] PRIMARY KEY CLUSTERED 
	(
		[person_id] ASC,
		[group_page_id] ASC
	)
	)
END
ELSE
BEGIN
	CREATE TABLE [mart].[bridge_group_page_person_new](
		[group_page_id] [int] NOT NULL,
		[person_id] [int] NOT NULL,
		[datasource_id] [int] NOT NULL,
		[insert_date] [smalldatetime] NULL,
	 CONSTRAINT [PK_bridge_group_page_person_new] PRIMARY KEY CLUSTERED 
	(
		[person_id] ASC,
		[group_page_id] ASC
	)
	) ON [MART]
END

--move the data
insert into [mart].[bridge_group_page_person_new]
select * from [mart].[bridge_group_page_person]

--Drop old table
DROP TABLE [mart].[bridge_group_page_person]

--Rename PK and Table
EXEC sp_rename 'mart.PK_bridge_group_page_person_new', 'PK_bridge_group_page_person'
EXEC sp_rename 'mart.bridge_group_page_person_new', 'bridge_group_page_person'

--recreate FKs
ALTER TABLE [mart].[bridge_group_page_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_group_page_person_dim_group_page] FOREIGN KEY([group_page_id])
REFERENCES [mart].[dim_group_page] ([group_page_id])

ALTER TABLE [mart].[bridge_group_page_person] CHECK CONSTRAINT [FK_bridge_group_page_person_dim_group_page]

ALTER TABLE [mart].[bridge_group_page_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_group_page_person_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])

ALTER TABLE [mart].[bridge_group_page_person] CHECK CONSTRAINT [FK_bridge_group_page_person_dim_person]

--new index to support selects based on the previous clustered key
CREATE NONCLUSTERED INDEX [IX_bridge_group_page_person_GroupPageId_PersonId] ON [mart].[bridge_group_page_person]
(
	[group_page_id] ASC,
	[person_id] ASC
)

