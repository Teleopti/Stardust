alter table status.customstatusstep
add CanBeDeleted bit not null
constraint df_canbedeleted default (1)
go

update status.customstatusstep set canbedeleted=0 where name='ETL'
