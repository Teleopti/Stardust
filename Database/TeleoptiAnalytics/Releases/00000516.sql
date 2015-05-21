alter table mart.dim_state_group
drop column is_deleted

alter table stage.stg_state_group
drop column is_deleted