Delete  TemplateMultisitePeriodDistribution FROM TemplateMultisitePeriodDistribution tm Inner join ChildSkill ck
on ck.Skill = tm.ChildSkill
WHERE ck.ParentSkill IS NULL

Delete  MultisitePeriodDistribution FROM MultisitePeriodDistribution mp Inner join ChildSkill ck
on ck.Skill = mp.ChildSkill
WHERE ck.ParentSkill IS NULL

Delete FROM ChildSkill WHERE ParentSkill IS NULL
