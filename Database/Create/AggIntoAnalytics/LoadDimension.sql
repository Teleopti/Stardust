--=============================
-- This script need to be executed in SQLCMD-mode with two parameters:
--:setvar SourceAgg TeleoptiCCC7Agg_Demo
--:setvar logobjectid 2
--ToDo: filter data on [log_object_id]
--=============================

----------------
-- update changes on acd_type
UPDATE dbo.acd_type
SET 
	acd_type_id		= source.acd_type_id, 
	acd_type_desc	= source.acd_type_desc
FROM
	$(SourceAgg).dbo.acd_type source
WHERE 
	dbo.acd_type.acd_type_id = source.acd_type_id

-- Insert new acd_type
INSERT INTO dbo.acd_type
	( 
	acd_type_id,
	acd_type_desc
	)
SELECT 
	acd_type_id		= source.acd_type_id, 
	acd_type_desc	= source.acd_type_desc
FROM
	$(SourceAgg).dbo.acd_type source
WHERE --filter out Existing acd_type
	NOT EXISTS (SELECT acd_type_id FROM dbo.acd_type target
					WHERE	target.acd_type_id = source.acd_type_id
				)

----------------
-- update changes on acd_type_detail
UPDATE dbo.acd_type_detail
SET 
	acd_type_id		= source.acd_type_id, 
	detail_id		= source.detail_id,
	detail_name		= source.detail_name,
	proc_name		= source.proc_name
FROM
	$(SourceAgg).dbo.acd_type_detail source
WHERE 
	dbo.acd_type_detail.acd_type_id = source.acd_type_id
AND 
	dbo.acd_type_detail.detail_id	= source.detail_id
	
-- Insert new acd_type
INSERT INTO dbo.acd_type_detail
	( 
	acd_type_id,
	detail_id,
	detail_name,
	proc_name
	)
SELECT 
	acd_type_id		= source.acd_type_id, 
	detail_id		= source.detail_id,
	detail_name		= source.detail_name,
	proc_name		= source.proc_name
FROM
	$(SourceAgg).dbo.acd_type_detail source
WHERE --filter out Existing acd_type
	NOT EXISTS (SELECT acd_type_id FROM dbo.acd_type_detail target
					WHERE	target.acd_type_id	= source.acd_type_id
					AND		target.detail_id	= source.detail_id
				)
----------------
-- update changes on log_object
UPDATE dbo.log_object
SET 
	log_object_id				= source.log_object_id, 
	acd_type_id					= source.acd_type_id,
	log_object_desc				= source.log_object_desc,
	logDB_name					= source.logDB_name, 
	intervals_per_day			= source.intervals_per_day,
	default_service_level_sec	= source.default_service_level_sec,
	default_short_call_treshold	= source.default_short_call_treshold
FROM
	$(SourceAgg).dbo.log_object source
WHERE 
	dbo.log_object.log_object_id = source.log_object_id
	AND dbo.log_object.log_object_id = $(logobjectid)

-- Insert new log_object
INSERT INTO dbo.log_object
	( 
	log_object_id,
	acd_type_id,
	log_object_desc,
	logDB_name,
	intervals_per_day,
	default_service_level_sec,
	default_short_call_treshold
	)
SELECT 
	log_object_id				= source.log_object_id, 
	acd_type_id					= source.acd_type_id,
	log_object_desc				= source.log_object_desc,
	logDB_name					= source.logDB_name, 
	intervals_per_day			= source.intervals_per_day,
	default_service_level_sec	= source.default_service_level_sec,
	default_short_call_treshold	= source.default_short_call_treshold
FROM
	$(SourceAgg).dbo.log_object source
WHERE --filter out Existing log_object
	NOT EXISTS (SELECT log_object_id FROM dbo.log_object target
					WHERE	target.log_object_id = source.log_object_id
				)
AND		source.log_object_id = $(logobjectid)
----------------
-- update changes on queues
UPDATE dbo.queues
SET 
	queue			= source.queue, 
	orig_desc		= source.orig_desc,
	log_object_id	= source.log_object_id,
	orig_queue_id	= source.orig_queue_id, 
	display_desc	= source.display_desc
FROM
	$(SourceAgg).dbo.queues source
WHERE 
	dbo.queues.queue = source.queue
AND dbo.queues.log_object_id = $(logobjectid)

-- Insert new queues
INSERT INTO dbo.queues
	( 
	queue,
	orig_desc,
	log_object_id,
	orig_queue_id,
	display_desc
	)
SELECT 
	queue			= source.queue, 
	orig_desc		= source.orig_desc,
	log_object_id	= source.log_object_id,
	orig_queue_id	= source.orig_queue_id, 
	display_desc	= source.display_desc
FROM
	$(SourceAgg).dbo.queues source
WHERE --filter out Existing queues
	NOT EXISTS (SELECT queue FROM dbo.queues target
					WHERE	target.queue = source.queue
				)
AND		source.log_object_id = $(logobjectid)
------------------
-- update changes on agent_info
UPDATE dbo.agent_info
SET 
	Agent_id		= source.Agent_id, 
	Agent_name		= source.Agent_name,
	is_active		= source.is_active,
	log_object_id	= source.log_object_id, 
	orig_agent_id	= source.orig_agent_id
FROM
	$(SourceAgg).dbo.agent_info source
WHERE 
	dbo.agent_info.Agent_id = source.Agent_id
AND dbo.agent_info.log_object_id = $(logobjectid)

-- Insert new agent_info
INSERT INTO dbo.agent_info
	( 
	Agent_id,
	Agent_name,
	is_active,
	log_object_id,
	orig_agent_id

	)
SELECT 
	Agent_id		= source.Agent_id, 
	Agent_name		= source.Agent_name,
	is_active		= source.is_active,
	log_object_id	= source.log_object_id, 
	orig_agent_id	= source.orig_agent_id
FROM
	$(SourceAgg).dbo.agent_info source
WHERE --filter out Existing agent_info
	NOT EXISTS (SELECT Agent_id FROM dbo.agent_info target
					WHERE	target.Agent_id = source.Agent_id
				)
AND		source.log_object_id = $(logobjectid)
------------------
-- update changes on ccc_system_info
UPDATE dbo.ccc_system_info
SET 
	id				= source.id, 
	[desc]			= source.[desc],
	int_value		= source.int_value,
	varchar_value	= source.varchar_value
FROM
	$(SourceAgg).dbo.ccc_system_info source
WHERE 
	dbo.ccc_system_info.id = source.id

-- Insert new ccc_system_info
INSERT INTO dbo.ccc_system_info
	( 
	id,
	[desc],
	int_value,
	varchar_value
	)
SELECT 
	id				= source.id, 
	[desc]			= source.[desc],
	int_value		= source.int_value,
	varchar_value	= source.varchar_value
FROM
	$(SourceAgg).dbo.ccc_system_info source
WHERE --filter out Existing ccc_system_info
	NOT EXISTS (SELECT id FROM dbo.ccc_system_info target
					WHERE	target.id = source.id
				)
