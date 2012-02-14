IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_archive_agent_report]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_archive_agent_report]
GO

CREATE  PROCEDURE [dbo].[p_archive_agent_report]
@date_from smalldatetime,
@date_to smalldatetime

AS

-- Skapa temptabell
CREATE TABLE #tmp_a_report (
	[id_nr] [int] NOT NULL ,
	[date] [smalldatetime] NOT NULL ,
	[interval] [smalldatetime] NOT NULL ,
	[agt_id] [int] NOT NULL ,
	[agt_name] [varchar] (50) NULL ,
	[acd_dn_number] [varchar] (10) NOT NULL ,
	[calls_answd] [int] NULL ,
	[non_actvy_calls] [int] NULL ,
	[acd_calls_xferd] [int] NULL ,
	[in_dn_calls] [int] NULL ,
	[out_dn_calls] [int] NULL ,
	[dn_calls_xferd] [int] NULL ,
	[short_calls] [int] NULL ,
	[total_acd_talk_time] [int] NULL ,
	[total_not_ready_time] [int] NULL ,
	[total_in_dn_time] [int] NULL ,
	[total_out_dn_time] [int] NULL ,
	[total_wait_time] [int] NULL ,
	[total_hold_time] [int] NULL ,
	[total_walk_time] [int] NULL ,
	[total_busy_time] [int] NULL ,
	[total_login_time] [int] NULL ,
	[total_consult_time] [int] NULL ,
	[total_staff_time] [int] NULL 
)


-- Starta transaktion
BEGIN TRAN

-- L�gg in i temptabellen
INSERT INTO #tmp_a_report
SELECT * 
FROM t_agent_report
WHERE date BETWEEN @date_from AND @date_to

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Sudda fr�n arkivet
DELETE AggDB_Archive.dbo.t_agent_report
FROM #tmp_a_report a1
INNER JOIN AggDB_Archive.dbo.t_agent_report a2
ON a1.date = a2.date AND a1.id_nr = a2.id_nr AND a1.interval = a2.interval AND a1.agt_id = a2.agt_id 
	AND a1.acd_dn_number COLLATE SQL_Latin1_General_CP1_CI_AS = a2.acd_dn_number COLLATE SQL_Latin1_General_CP1_CI_AS


IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- L�gg in i arkivet
INSERT INTO AggDB_Archive.dbo.t_agent_report
SELECT *
FROM #tmp_a_report

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Sudda fr�n loggen
DELETE t_agent_report
FROM #tmp_a_report a1
INNER JOIN t_agent_report a2
ON a1.date = a2.date AND a1.id_nr = a2.id_nr AND a1.interval = a2.interval AND a1.agt_id = a2.agt_id 
	AND a1.acd_dn_number COLLATE SQL_Latin1_General_CP1_CI_AS = a2.acd_dn_number COLLATE SQL_Latin1_General_CP1_CI_AS
--	AND CAST(a1.acd_dn_number AS varchar(10)) COLLATE SQL_Latin1_General_CP1_CI_AS) = CAST(a2.acd_dn_number AS varchar(10)) COLLATE SQL_Latin1_General_CP1_CI_AS)

IF @@ERROR <> 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
ELSE
BEGIN
	COMMIT TRAN
END

GO

