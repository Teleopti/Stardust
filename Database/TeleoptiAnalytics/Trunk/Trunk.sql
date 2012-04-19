-----------------  
---Name: Jonas N
---Date: 2012-04-04
---Desc: Add a table used by ETL service and ETL Tool. 
---			Before a ETL job is started a check is made if another ETL is running. 
---			When a job is started a transaction lock is set on this table.
-----------------

CREATE TABLE [mart].[sys_etl_running_lock](
	[id] [int] NOT NULL,
	[computer_name] [nvarchar](255) NOT NULL,
	[start_time] [datetime] NOT NULL,
	[job_name] [nvarchar](100) NOT NULL,
	[is_started_by_service] [bit] NOT NULL,
 CONSTRAINT [PK_sys_etl_running_lock] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
) ON [PRIMARY]

GO