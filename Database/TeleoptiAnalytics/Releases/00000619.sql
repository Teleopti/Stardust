-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-05-08
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

CREATE TABLE dbo.Tmp_DBA_VirtualFileStats
	(
	id int NOT NULL IDENTITY (1, 1),
	database_id int NOT NULL,
	file_id int NOT NULL,
	ServerName varchar(255) NOT NULL,
	DatabaseName varchar(255) NOT NULL,
	PhysicalName varchar(255) NOT NULL,
	num_of_reads bigint NULL,
	num_of_reads_from_start bigint NULL,
	num_of_writes bigint NULL,
	num_of_writes_from_start bigint NULL,
	num_of_bytes_read bigint NULL,
	num_of_bytes_read_from_start bigint NULL,
	num_of_bytes_written bigint NULL,
	num_of_bytes_written_from_start bigint NULL,
	io_stall bigint NULL,
	io_stall_from_start bigint NULL,
	io_stall_read_ms bigint NULL,
	io_stall_read_ms_from_start bigint NULL,
	io_stall_write_ms bigint NULL,
	io_stall_write_ms_from_start bigint NULL,
	RecordedDateTime datetime NULL,
	interval_ms bigint NULL,
	FirstMeasureFromStart bit NULL
	)  ON [PRIMARY]
GO
DROP TABLE dbo.DBA_VirtualFileStats
GO
EXECUTE sp_rename N'dbo.Tmp_DBA_VirtualFileStats', N'DBA_VirtualFileStats', 'OBJECT' 
GO
ALTER TABLE dbo.DBA_VirtualFileStats ADD CONSTRAINT
	PK_DBA_VirtualFileStats PRIMARY KEY NONCLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX CIX_DBA_VirtualFileStats_RecordedDateTime ON dbo.DBA_VirtualFileStats
	(
	RecordedDateTime
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO



DROP TABLE dbo.DBA_VirtualFileStatsHistory
GO
CREATE TABLE dbo.DBA_VirtualFileStatsHistory
	(
	RecordID int NOT NULL IDENTITY (1, 1),
	id int NOT NULL,
	database_id int NOT NULL,
	file_id int NOT NULL,
	ServerName varchar(255) NOT NULL,
	DatabaseName varchar(255) NOT NULL,
	PhysicalName varchar(255) NOT NULL,
	num_of_reads bigint NULL,
	num_of_reads_from_start bigint NULL,
	num_of_writes bigint NULL,
	num_of_writes_from_start bigint NULL,
	num_of_bytes_read bigint NULL,
	num_of_bytes_read_from_start bigint NULL,
	num_of_bytes_written bigint NULL,
	num_of_bytes_written_from_start bigint NULL,
	io_stall bigint NULL,
	io_stall_from_start bigint NULL,
	io_stall_read_ms bigint NULL,
	io_stall_read_ms_from_start bigint NULL,
	io_stall_write_ms bigint NULL,
	io_stall_write_ms_from_start bigint NULL,
	RecordedDateTime datetime NULL,
	interval_ms bigint NULL,
	FirstMeasureFromStart bit NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.DBA_VirtualFileStatsHistory SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.DBA_VirtualFileStatsHistory ON
GO
ALTER TABLE dbo.DBA_VirtualFileStatsHistory ADD CONSTRAINT
	PK_DBA_VirtualFileStatsHistory PRIMARY KEY NONCLUSTERED 
	(
	RecordID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX CIX_DBA_VirtualFileStatsHistory_RecordedDateTime ON dbo.DBA_VirtualFileStatsHistory
	(
	RecordedDateTime
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

