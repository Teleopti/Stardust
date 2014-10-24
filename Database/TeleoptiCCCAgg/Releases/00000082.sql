----------------  
--Name: Anders F  
--Date: 2009-03-20  
--Desc: Correct initial data for new aggs  
----------------  
IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 0)
	INSERT INTO acd_type
	VALUES (0,'Default')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 1)
	INSERT INTO acd_type
	VALUES (1,'Avaya Definity CMS')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 2)
	INSERT INTO acd_type
	VALUES (2,'Nortel Symposium 3-4.0 Skillset')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 3)
	INSERT INTO acd_type
	VALUES (3,'Nortel Symposium 1.5 Skillset')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 4)
	INSERT INTO acd_type
	VALUES (4,'Nortel Symposium 3-4.0 Application')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 5)
	INSERT INTO acd_type
	VALUES (5,'Nortel Symposium 1.5 Application')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 6)
	INSERT INTO acd_type
	VALUES (6,'Siemens ProCenter Advanced')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 7)
	INSERT INTO acd_type
	VALUES (7,'Siemens ProCenter Entry')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 8)
	INSERT INTO acd_type
	VALUES (8,'Ericsson Solidus E-Care')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 9)
	INSERT INTO acd_type
	VALUES (9,'Ericsson CCM')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 10)
	INSERT INTO acd_type
	VALUES (10,'Interactive Intelligence Interaction center')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 11)
	INSERT INTO acd_type
	VALUES (11,'Telia VCC 7.5')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 12)
	INSERT INTO acd_type
	VALUES (12,'ClearIT MCC')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 13)
	INSERT INTO acd_type
	VALUES (13,'WebTrump - ccBridge')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 14)
	INSERT INTO acd_type
	VALUES (14,'Nokia DX200')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 15)
	INSERT INTO acd_type
	VALUES (15,'Cisco ICM5')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 16)
	INSERT INTO acd_type
	VALUES (16,'Advoco')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 17)
	INSERT INTO acd_type
	VALUES (17,'TeliaSonera CallGuide 4')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 18)
	INSERT INTO acd_type
	VALUES (18,'Alcatel')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 19)
	INSERT INTO acd_type
	VALUES (19,'Telia VCC 7.6')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 20)
	INSERT INTO acd_type
	VALUES (20,'Telia VCC 8')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 21)
	INSERT INTO acd_type
	VALUES (21,'CDS')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 22)
	INSERT INTO acd_type
	VALUES (22,'Wicom')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 23)
	INSERT INTO acd_type
	VALUES (23,'Altitude 6.2')

IF NOT EXISTS (SELECT 1 FROM acd_type WHERE acd_type_id = 24)
	INSERT INTO acd_type
	VALUES (24,'Wicomrt')
GO

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 0 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (0,1,'Queue logg','Proc name')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 0 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (0,2,'Agent logg','Proc name')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 0 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (0,3,'Goal results','Proc name')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 1 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (1,1,'Queue logg','p_cms_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 1 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (1,2,'Agent logg','p_cms_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 1 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (1,3,'Goal results','p_cms_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 2 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (2,1,'Queue logg','p_sym40_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 2 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (2,2,'Agent logg','p_sym40_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 2 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (2,3,'Goal results','p_sym40_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 3 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (3,1,'Queue logg','p_sym15_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 3 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (3,2,'Agent logg','p_sym15_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 3 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (3,3,'Goal results','p_sym15_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 4 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (4,1,'Queue logg','p_symapp40_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 4 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (4,2,'Agent logg','p_symapp40_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 4 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (4,3,'Goal results','p_symapp40_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 5 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (5,1,'Queue logg','p_symapp15_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 5 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (5,2,'Agent logg','p_symapp15_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 5 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (5,3,'Goal results','p_symapp15_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 6 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (6,1,'Queue logg','p_siemA_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 6 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (6,2,'Agent logg','p_siemA_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 7 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (7,1,'Queue logg','p_siemE_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 7 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (7,2,'Agent logg','p_siemE_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 7 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (7,3,'Goal results','p_siemE_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 8 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (8,1,'Queue logg','p_solidus_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 8 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (8,2,'Agent logg','p_solidus_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 8 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (8,3,'Goal results','p_solidus_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 9 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (9,1,'Queue logg','p_ccm_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 9 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (9,2,'Agent logg','p_ccm_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 9 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (9,3,'Goal results','p_ccm_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 10 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (10,1,'Queue logg','p_ic_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 10 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (10,2,'Agent logg','p_ic_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 10 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (10,3,'Goal results','p_ic_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 11 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (11,1,'Queue logg','p_vcc_7_5_insert_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 11 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (11,2,'Agent Logg','p_vcc_7_5_insert_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 11 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (11,3,'Goal results','p_vcc_7_5_insert_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 12 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (12,1,'Queue logg','p_mcc_insert_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 12 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (12,2,'Agent logg','p_mcc_insert_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 12 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (12,3,'Goal results','p_mcc_insert_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 13 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (13,1,'Queue logg','p_bridge_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 13 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (13,2,'Agent logg','p_bridge_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 13 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (13,3,'Goal results','p_bridge_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 14 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (14,1,'Queue logg','p_dx200_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 14 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (14,2,'Agent logg','p_dx200_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 14 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (14,3,'Goal results','p_dx200_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 15 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (15,1,'Queue logg','p_cisco50_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 15 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (15,2,'Agent logg','p_cisco50_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 15 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (15,3,'Goal results','p_cisco50_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 16 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (16,1,'Queue logg','p_advoco_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 16 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (16,2,'Agent logg','p_advoco_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 16 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (16,3,'Goal results','p_advoco_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 18 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (18,1,'Queue logg','p_alcatel53_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 19 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (19,1,'Queue logg','p_vcc_7_6_insert_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 19 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (19,2,'Agent logg','p_vcc_7_6_insert_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 19 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (19,3,'Goal results','p_vcc_7_6_insert_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 20 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (20,1,'Queue logg','p_vcc8_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 20 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (20,2,'Agent logg','p_vcc8_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 20 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (20,3,'Goal results','p_vcc8_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 21 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (21,1,'Queue logg','p_cds_insert_queuelogg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 21 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (21,2,'Agent logg','p_cds_insert_agentlogg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 21 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (21,3,'Goal results','p_cds_insert_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 23 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (23,1,'Queue logg','p_altitude6_2_insert_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 23 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (23,2,'Agent logg','p_altitude6_2_insert_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 23 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (23,3,'Agent logg','p_altitude6_2_insert_goal_results')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 24 AND detail_id = 1)
	INSERT INTO acd_type_detail
	VALUES (24,1,'Queue logg','p_wicomrt_2_insert_queue_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 24 AND detail_id = 2)
	INSERT INTO acd_type_detail
	VALUES (24,2,'Agent logg','p_wicomrt_2_insert_agent_logg')

IF NOT EXISTS (SELECT 1 FROM acd_type_detail WHERE acd_type_id = 24 AND detail_id = 3)
	INSERT INTO acd_type_detail
	VALUES (24,3,'Goal results','p_wicomrt_2_insert_goalresultst_logg')
GO

IF NOT EXISTS (SELECT 1 FROM ccc_system_info)
	INSERT INTO ccc_system_info
	VALUES (1, 'CCC intervals per day', 96,NULL)
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (82,'7.0.82') 
