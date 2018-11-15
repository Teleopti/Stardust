alter table planninggroupsettings 
drop constraint FK_PlanningGroupSettings_Person_UpdatedBy

alter table planninggroupsettings 
drop constraint FK_PlanningGroupSettings_BusinessUnit

DROP INDEX UQ_PlanningGroupSettings_DefaultSettings ON PlanningGroupSettings

alter table planninggroupsettings
drop column updatedby

alter table planninggroupsettings
drop column updatedon

alter table planninggroupsettings
drop column businessunit

update planninggroupsettings
set priority=-1
where defaultsettings = 1

EXEC sp_rename 'planninggroupsettings.PlanningGroup', 'Parent', 'COLUMN'; 



