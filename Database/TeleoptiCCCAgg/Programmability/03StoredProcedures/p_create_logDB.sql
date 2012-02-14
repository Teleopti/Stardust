IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_create_logDB]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_create_logDB]
GO

CREATE  procedure [dbo].[p_create_logDB]

/**********************************************/
/* To add logDB information to AggDB */
/**********************************************/

@acd_type_id int,
@logDB varchar(50)

as


declare @id int
select @id = isnull(max(log_object_id),0) + 1 from log_object

/*Check if ACD Type exists*/

if exists (select 1 from acd_type where acd_type_id = @acd_type_id)
begin
	insert into log_object(log_object_id, acd_type_id, log_object_desc,  logDB_name,intervals_per_day)
	select @id, @acd_type_id, acd_type_desc, @logDB,96 
	from acd_type
	where acd_type_id = @acd_type_id

	insert into log_object_detail(log_object_id, detail_id, detail_desc, proc_name, int_value, date_value)
	select @id,detail_id, detail_name, proc_name,0,'2002-01-01' 
	from acd_type_detail
	where acd_type_id = @acd_type_id
end

GO

