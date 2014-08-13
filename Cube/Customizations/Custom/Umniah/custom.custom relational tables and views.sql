--Only works for DEMO!
--USE TeleoptiAnalytics_Demo
--create custom schema
if not exists(select * from sys.schemas where Name = N'Custom')
begin 
exec('CREATE SCHEMA [Custom] AUTHORIZATION [dbo]')
end
GO

--load agg data into mart
TRUNCATE TABLE mart.fact_quality
TRUNCATE TABLE mart.fact_agent_queue
exec mart.etl_fact_agent_queue_load '2001-01-01','2014-12-31',6
exec mart.etl_fact_quality_load '2001-01-01','2014-12-31',8

--fake some sales data
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[custom].[fact_sales]'))
DROP VIEW [custom].[fact_sales]
GO
CREATE VIEW [custom].[fact_sales]
AS
SELECT
	f.acd_login_id,
	f.date_id,
	sum(f.talk_time_s) as 'sales_value',
	sum(f.answered_calls) as 'HITS'
FROM mart.fact_agent_queue f
GROUP BY f.acd_login_id,f.date_id
GO

--create table to hold wighted values by key
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[custom].[WeightedMeasure]') AND type in (N'U'))
DROP TABLE [custom].[WeightedMeasure]
GO
CREATE TABLE [custom].[WeightedMeasure](
	[factor_name] nvarchar(200) NOT NULL,
	[measure_name] nvarchar(200) NOT NULL,
	[weighted_value] decimal(8,4) NOT NULL
)
ALTER TABLE [custom].[WeightedMeasure] ADD  CONSTRAINT [PK_WeightedMeasure] PRIMARY KEY CLUSTERED 
(
	[measure_name] ASC,
	[factor_name] ASC
)
GO



--Custom QM "fact table" as a PIVOT view over specific Questionaries
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[custom].[v_fact_QM]'))
DROP VIEW [custom].[v_fact_QM]
GO
CREATE VIEW [custom].[v_fact_QM]
AS

	SELECT	date_id,
			acd_login_id,
			ISNULL(AHT,0) as 'AHT',
			ISNULL(FCR,0) as 'FCR'
	FROM (
		SELECT 
			f.date_id,
			f.acd_login_id,
			CASE d.quality_quest_name
				WHEN 'Average Handling Time' THEN 'AHT'
				WHEN 'First Call Resolution (Inbound tech support)' THEN 'FCR'
			END as [factor_name], 
			isnull(f.score,0) as [score]
		FROM mart.fact_quality f
		INNER JOIN mart.dim_quality_quest d
			ON d.quality_quest_id = f.quality_quest_id
	) as s
	PIVOT
	(
		SUM(score)
		FOR [factor_name] IN (AHT,FCR)
	) AS result

GO

--A function to get the weighted value by key
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'custom.GetWeight') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION custom.GetWeight
GO

--A function to get the weighted value by key
--TODO: Check the performance on this one!! maybe it can be solved by straight on viewes instead?
CREATE FUNCTION custom.GetWeight(@factor_name nvarchar(200), @measure_name nvarchar(200))
RETURNS decimal(8,4)
AS BEGIN

    DECLARE @weighted_value decimal(8,4)

	SELECT
		@weighted_value = weighted_value
	FROM [custom].[WeightedMeasure]
	WHERE factor_name=@factor_name
	AND measure_name=@measure_name

    RETURN @weighted_value
END
GO

--fake some sales data into a "Sales" fact-table
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[custom].[v_fact_sales]'))
DROP VIEW [custom].[v_fact_sales]
GO
CREATE VIEW [custom].[v_fact_sales]
AS
SELECT
	f.acd_login_id,
	f.date_id,
	isnull(sum(f.talk_time_s),0) as 'sales_value',
	isnull(sum(f.answered_calls),0) as 'HITS'
FROM mart.fact_agent_queue f
GROUP BY f.acd_login_id,f.date_id
GO

--create a Measure Group for all custom measures
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[custom].[v_fact_measureGroup]'))
DROP VIEW [custom].[v_fact_measureGroup]
GO
CREATE VIEW custom.v_fact_measureGroup
AS

SELECT
	acd_login_id,
	date_id,
	sum(AHT) as 'AHT',
	sum(FCR) as 'FCR',
	sum(SALES) as 'SALES',
	sum(HITS)  as 'HITS'
FROM (
	SELECT
		q.acd_login_id,
		q.date_id,
		ISNULL(q.AHT,0) as 'AHT',
		ISNULL(q.FCR,0) as 'FCR',
		0 as 'SALES',
		0 as 'HITS'
	FROM [custom].[v_fact_QM] q

	UNION ALL

	SELECT
		s.acd_login_id,
		s.date_id,
		0 as 'AHT',
		0 as 'FCR',
		ISNULL(s.sales_value,0) as 'SALES',
		ISNULL(s.HITS,0) as 'HITS'
	FROM [custom].[fact_sales] s
	) total
GROUP BY acd_login_id,date_id
GO

--=====================================
--one way to handle the "dynamic" factor
--=====================================
INSERT [custom].[WeightedMeasure]
SELECT 'AHT','Measure1',0.6
UNION ALL
SELECT 'FCR','Measure1',0.2
UNION ALL
SELECT 'SALES','Measure1',0.8
UNION ALL
SELECT 'AHT','Measure2',0.4
UNION ALL
SELECT 'FCR','Measure2',0.5
UNION ALL
SELECT 'HITS','Measure2',12
UNION ALL
SELECT 'SALES','Measure2',0.63

--The final "fact_table"
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[custom].[v_fact_calculated_measures]'))
DROP VIEW [custom].[v_fact_calculated_measures]
GO
CREATE VIEW [custom].[v_fact_calculated_measures]
AS
SELECT
		acd_login_id,
		date_id,
		AHT,
		FCR,
		SALES
		HITS,
		--Measure1 = AHT*0.6 + FCR*0.2 + SALES*0.8
			 AHT*custom.GetWeight('AHT','Measure1')
			+FCR*custom.GetWeight('FCR','Measure1')
			+SALES*custom.GetWeight('SALES','Measure1')
		as 'Measure1',
		--Measure2 = AHT*0.4 + FCR*0.5 + SALES*0.63/HITS/12
			 AHT*custom.GetWeight('AHT','Measure2')
			+FCR*custom.GetWeight('FCR','Measure2')
			+CASE HITS
				WHEN 0 THEN 0
				ELSE SALES*custom.GetWeight('SALES','Measure2')/HITS/custom.GetWeight('HITS','Measure2')
			END as 'Measure2'
		
		--maybe 3-10 fixed Measures/formulas were the formulas are stored in DB, only to be touched by the partner
FROM custom.v_fact_measureGroup
GO

GO


SELECT * FROM [custom].[fact_Sales]
SELECT * FROM [custom].[v_fact_calculated_measures]
