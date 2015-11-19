----------------  
--Name: Asad Mirza
--Date: 2015-11-19
--Desc: migration script for new web permissions
----------------
begin
declare @nu datetime
declare @webPermissionId as nvarchar(100)

set @nu = GETUTCDATE()

select @webPermissionId = CONVERT(uniqueidentifier,Id) FROM ApplicationFunction
where FunctionCode = 'WebPermissions' 

  insert into ApplicationFunctionInRole ( ApplicationRole,ApplicationFunction,InsertedOn)
   select afir.ApplicationRole, @webPermissionId as ApplicationFunction,
    @nu as InsertedOn   from ApplicationFunctionInRole afir,
	ApplicationFunction af
	where afir.ApplicationFunction = af.id
	and af.FunctionCode  = 'Permission'
end
