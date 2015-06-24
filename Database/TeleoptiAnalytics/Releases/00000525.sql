--David Jonsson
--bug #33855

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[bridge_acd_login_person]') AND name = N'IX_bridge_acd_login_person_person_acd_login_id')
CREATE NONCLUSTERED INDEX [IX_bridge_acd_login_person_person_acd_login_id]
ON [mart].[bridge_acd_login_person] ([person_id])
INCLUDE ([acd_login_id])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_SkillSet')
CREATE NONCLUSTERED INDEX [IX_dim_person_SkillSet]
ON [mart].[dim_person] ([skillset_id],[valid_from_date],[valid_to_date])
INCLUDE ([person_id])
GO
