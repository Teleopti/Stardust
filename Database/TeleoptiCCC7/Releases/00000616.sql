alter table tenant.personinfo
drop constraint FK_PersonInfo_Tenant 

alter table tenant.personinfo
add constraint FK_PersonInfo_Tenant 
foreign key (Tenant) 
references Tenant.Tenant (Id)
on delete cascade
