IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[ReadModel].[v_ScheduleProjectionReadOnlyRTA]'))
DROP VIEW [ReadModel].[v_ScheduleProjectionReadOnlyRTA]
GO

DROP INDEX [CIX_ScheduleProjectionReadOnly] ON [ReadModel].[ScheduleProjectionReadOnly] WITH ( ONLINE = OFF )
GO

ALTER TABLE [ReadModel].[ScheduleProjectionReadOnly] DROP CONSTRAINT [PK_ScheduleProjectionReadOnly]
GO
ALTER TABLE [ReadModel].[ScheduleProjectionReadOnly] DROP CONSTRAINT [DF_ScheduleProjectionReadOnly_Id]
GO
ALTER TABLE [ReadModel].[ScheduleProjectionReadOnly] DROP COLUMN [Id]
GO

--#40795: delete potential duplicates. Keep latest InsertedOn
WITH cte as(
  SELECT ROW_NUMBER() OVER (PARTITION BY BelongsToDate,PersonId,StartDateTime
                            ORDER BY  InsertedOn DESC ) RN
  FROM   [ReadModel].[ScheduleProjectionReadOnly]
  )
delete from cte where RN>1
GO

ALTER TABLE [ReadModel].[ScheduleProjectionReadOnly]
ADD CONSTRAINT [PK_ScheduleProjectionReadOnly] PRIMARY KEY CLUSTERED (
	[PersonId] ASC,
	[BelongsToDate] ASC,
	[StartDateTime] ASC
);
GO  
