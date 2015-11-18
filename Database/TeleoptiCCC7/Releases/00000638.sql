----------------  
--Name: Zhiping
--Date: 2015-11-16
--Desc: Store application roles for seats
----------------

create table [dbo].[ApplicationRolesForSeat] (
	[Seat] [uniqueidentifier] not null,
	[ApplicationRole] [uniqueidentifier] not null,
	[UpdatedOn] [datetime] null
)

alter table [dbo].[ApplicationRolesForSeat] with check add
constraint [FK_ApplicationRolesForSeat_Seat] foreign key ([Seat])
references [dbo].[Seat] ([Id]);
go

alter table [dbo].[ApplicationRolesForSeat] with check add  
constraint [FK_ApplicationRolesForSeat_ApplicationRole] foreign key ([ApplicationRole])
references [dbo].[ApplicationRole] ([Id]);
go