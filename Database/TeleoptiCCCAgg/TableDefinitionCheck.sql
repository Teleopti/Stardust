-- Run this script in the Agg-Db that are to be upgraded.
-- If OK, execute the section at the botten (Commented out)
-- If not OK, don't upgrade!

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[agent_info]') AND name = N'test_uix_orig_agent_id')
CREATE UNIQUE NONCLUSTERED INDEX [test_uix_orig_agent_id] ON [dbo].[agent_info] 
(
	[log_object_id] ASC,
	[orig_agent_id] ASC
)

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[agent_info]') AND name = N'test_uix_orig_agent_id')
DROP INDEX [test_uix_orig_agent_id] ON [dbo].[agent_info] WITH ( ONLINE = OFF )

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[queues]') AND name = N'test_uix_orig_queue_id')
CREATE UNIQUE NONCLUSTERED INDEX [test_uix_orig_queue_id] ON [dbo].[queues] 
(
	[log_object_id] ASC,
	[orig_queue_id] ASC
)

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[queues]') AND name = N'test_uix_orig_queue_id')
DROP INDEX [test_uix_orig_queue_id] ON [dbo].[queues] WITH ( ONLINE = OFF )

SET NOCOUNT ON
	DECLARE @errorNumber int
	DECLARE @SchemaName sysname
	DECLARE @TableName sysname
	DECLARE @isPK int
	DECLARE @Key1 nvarchar (128)
	DECLARE @Key2 nvarchar (128)
	DECLARE @Key3 nvarchar (128)
	DECLARE @Key4 nvarchar (128)

	SET @errorNumber = 0
	
	SET @SchemaName='dbo'
	SET @TableName='agent_logg'
	SET @isPK=1
	SET @Key1='date_from'
	SET @Key2='agent_id'
	SET @Key3='queue'
	SET @Key4='interval'

	DECLARE @countColumns int
	DECLARE @TableCheckSumCCC7 table(
		TableName sysname NOT NULL,
		ColumnName sysname NOT NULL,
		DataType int);

	DECLARE @TableCheckSumCCC6 table(
		TableName sysname NOT NULL,
		ColumnName sysname NOT NULL,
		DataType int);

--Current CCC6 tables DDL
	INSERT INTO @TableCheckSumCCC6
	SELECT so.name, sc.name,sc.user_type_id
	FROM sys.objects so
	inner join sys.columns sc
	on so.object_id=sc.object_id
	WHERE so.type = 'U'
	AND so.name IN ('agent_info','agent_logg','ccc_system_info','queue_logg','queues')
	
--Upcoming CCC7 tables DDL
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_info','Agent_id','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_info','Agent_name','167')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_info','is_active','104')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_info','log_object_id','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_info','orig_agent_id','167')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','admin_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','agent_id','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','agent_name','231')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','answ_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','avail_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','date_from','58')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','direct_in_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','direct_in_call_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','direct_out_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','direct_out_call_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','interval','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','pause_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','queue','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','talking_call_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','tot_work_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','transfer_out_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','wait_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('agent_logg','wrap_up_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('ccc_system_info','desc','167')
	INSERT INTO @TableCheckSumCCC7 VALUES ('ccc_system_info','id','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('ccc_system_info','int_value','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('ccc_system_info','varchar_value','175')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','aband_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','aband_short_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','aband_within_sl_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','ans_servicelevel_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','answ_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','avg_avail_member_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','date_from','58')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','interval','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','offd_direct_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','overflow_in_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','overflow_out_call_cnt','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','queue','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','queued_aband_longest_que_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','queued_and_aband_call_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','queued_and_answ_call_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','queued_answ_longest_que_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','talking_call_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','wait_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queue_logg','wrap_up_dur','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queues','display_desc','167')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queues','log_object_id','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queues','orig_desc','167')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queues','orig_queue_id','56')
	INSERT INTO @TableCheckSumCCC7 VALUES ('queues','queue','56')

IF (
	--Check correct tables structure
	SELECT COUNT(*) FROM @TableCheckSumCCC7 ccc7
	FULL OUTER JOIN @TableCheckSumCCC6 ccc6
	ON ccc6.TableName = ccc7.TableName AND ccc6.ColumnName = ccc7.ColumnName AND ccc6.DataType=ccc7.DataType
	WHERE ccc6.ColumnName IS NULL OR ccc7.ColumnName IS NULL OR ccc6.DataType IS NULL OR ccc7.DataType IS NULL
	) <> 0
BEGIN	
	SET @errorNumber = 1
	PRINT 'table structures are not correct!'
	SELECT
			ccc7.TableName as 'ccc7.TableName',ccc7.ColumnName as 'ccc7.ColumnName',ccc7.DataType as 'ccc7.DataType',
			ccc6.TableName as 'ccc6.TableName',ccc6.ColumnName as 'ccc6.ColumnName',ccc6.DataType as 'ccc6.DataType'
		FROM @TableCheckSumCCC7 ccc7
		FULL OUTER JOIN @TableCheckSumCCC6 ccc6
		ON ccc6.TableName = ccc7.TableName AND ccc6.ColumnName = ccc7.ColumnName AND ccc6.DataType=ccc7.DataType
		WHERE ccc6.ColumnName IS NULL OR ccc7.ColumnName IS NULL OR ccc6.DataType IS NULL OR ccc7.DataType IS NULL
		ORDER BY ccc7.TableName,ccc6.TableName
END 
	
IF	(
	SELECT COUNT(*)
	FROM sys.indexes as si 
	LEFT JOIN sys.objects as so on so.object_id=si.object_id 
	WHERE schema_name(schema_id) = @SchemaName
	AND si.object_id = object_id(@TableName)
	AND si.type = @isPK
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,1) = @Key1
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,2) = @Key2
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,3) = @Key3
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,4) = @Key4
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,5) IS NULL
	) <> 1
BEGIN
	SET @errorNumber = 2
	PRINT 'Primary key structure on agent_logg is not correct'
	
END

IF @errorNumber = 0
	print 'This Agg is OK! :-)'
ELSE
	print 'errors found!'

/* If all OK then implement:
CREATE TABLE [dbo].[DatabaseVersion](
	[BuildNumber] [int] NOT NULL,
	[SystemVersion] [varchar](100) NOT NULL,
	[AddedDate] [datetime] NOT NULL,
	[AddedBy] [varchar](1000) NOT NULL
) ON [PRIMARY]


ALTER TABLE [dbo].[DatabaseVersion] ADD  CONSTRAINT [PK_DatabaseVersion] PRIMARY KEY CLUSTERED 
(
	[BuildNumber] ASC
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DatabaseVersion] ADD  CONSTRAINT [DF_DatabaseVersion_AddedDate]  DEFAULT (getdate()) FOR [AddedDate]
GO

ALTER TABLE [dbo].[DatabaseVersion] ADD  CONSTRAINT [DF_DatabaseVersion_AddedBy]  DEFAULT (suser_sname()) FOR [AddedBy]
GO

INSERT INTO dbo.DatabaseVersion(BuildNumber, SystemVersion)
VALUES (138,'7.0.138') 

*/
