create table dbo.SkillFilter (
  Id uniqueidentifier not null,
  Skill uniqueidentifier not null
)
alter table dbo.SkillFilter ADD CONSTRAINT PK_SkillFilter PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
alter table dbo.SkillFilter 
add constraint FK_SkillFilter_Skill
foreign key (Skill) references dbo.Skill