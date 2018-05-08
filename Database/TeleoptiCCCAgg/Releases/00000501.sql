-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-05-08
-- Desc: Fixing PK's. VSTS #75391 (Old unused tables probably from Teleopti CCC version 6)
-----------------------------------------------------------
DROP TABLE dbo.t_log_NET_data_genesys_ctia_agent
DROP TABLE dbo.t_log_NET_data_solidus_agent
DROP TABLE dbo.t_log_NET_data_solidus_logg
DROP TABLE dbo.t_log_NET_data_symposium_agent
DROP TABLE dbo.t_log_oasis_realtime
DROP TABLE dbo.t_log_solidus_realtime
DROP TABLE dbo.t_log_symposium_realtime
DROP TABLE dbo.t_rta_display_data
DROP TABLE dbo.t_log_alcatel_realtime
DROP TABLE dbo.t_log_avaya_realtime
DROP TABLE dbo.t_log_callguide_realtime
DROP TABLE dbo.t_log_NET_data_avaya_agent
DROP TABLE dbo.t_log_NET_data_callguide_agent
