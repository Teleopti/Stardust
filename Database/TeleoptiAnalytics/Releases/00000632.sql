drop table mart.LastEtlPing
go

create table mart.LastEtlPing (id int not null, date datetime not null)
go
create clustered index [CIX_LastEtlPing] on mart.lastetlping (id)
go

insert into mart.LastEtlPing (id, date) values (1, '2015-1-1')