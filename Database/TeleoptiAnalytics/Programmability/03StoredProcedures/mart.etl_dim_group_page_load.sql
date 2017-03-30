IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_group_page_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_group_page_load]
GO


-- =============================================
-- Author:		DJ
-- Create date: 2010-08-30
--
-- Change Log
-----------------------------------------
--	UpdateDate	By	Description				
-----------------------------------------
--	2010-08-30	DJ	Adding Business unit
--	2010-09-02	DJ	Adding group_id OVER (PARTION)
--  2010-09-05  JN  Refactor way of creating group_id
--	2010-09-17	KJ	Added truncate of bridge table,reset identity and DISTINCT on insert.
--  2010-12-15  HG	Changed the TRUNCATE to DELETE FROM to only delete stuff in current BusinessUnit
-- 2011-04-11	JN  Inject current BU and complete change of load. Instead of always deleting all
--					for current bu and reload, we now do delete, update and insert.
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_group_page_load] 
@business_unit_code uniqueidentifier	
AS
---------------------------------------------------------------------------

DECLARE @counter int
DECLARE @group_page_code uniqueidentifier
DECLARE @group_page_name nvarchar(100)
DECLARE @group_page_name_resource_key nvarchar(100)
DECLARE @group_code uniqueidentifier
DECLARE @group_name nvarchar(1024)
DECLARE @datasource_id int
DECLARE @note_resource_name nvarchar(100)

SET @note_resource_name = 'Note'

-- Delete all records for persons in current bu from bridge table
DELETE FROM [mart].[bridge_group_page_person]
	FROM [mart].[bridge_group_page_person] AS bgpp
	INNER JOIN [mart].[dim_person] AS dp
		ON bgpp.person_id = dp.person_id
		AND dp.business_unit_code = @business_unit_code

-- Delete non custom group pages (excluding Note) from dimension that does not exist in stage.
DELETE FROM mart.dim_group_page
FROM mart.dim_group_page dgp
WHERE dgp.group_is_custom = 0
	AND dgp.business_unit_code = @business_unit_code
	AND dgp.group_page_name_resource_key <> @note_resource_name
	AND NOT EXISTS	(
						SELECT * FROM stage.stg_group_page_person sgp
						WHERE sgp.group_is_custom = 0
							AND sgp.group_page_name_resource_key = dgp.group_page_name_resource_key
							AND sgp.group_code = dgp.group_code
							AND sgp.business_unit_code = dgp.business_unit_code
					)

-- Delete Note group page from dimension that does not exist in stage.
DELETE FROM mart.dim_group_page
FROM mart.dim_group_page dgp
WHERE dgp.group_is_custom = 0
	AND dgp.business_unit_code = @business_unit_code
	AND dgp.group_page_name_resource_key = @note_resource_name
	AND NOT EXISTS	(
						SELECT * FROM stage.stg_group_page_person sgp
						WHERE sgp.group_is_custom = 0
							AND sgp.group_page_name_resource_key = dgp.group_page_name_resource_key
							AND sgp.group_name COLLATE Latin1_General_CS_AS = dgp.group_name COLLATE Latin1_General_CS_AS
							AND sgp.business_unit_code = dgp.business_unit_code
					)

-- Delete optional column group pages from dimension that does not exist in stage.
DELETE FROM mart.dim_group_page
FROM mart.dim_group_page dgp
WHERE dgp.group_is_custom = 0
	AND dgp.business_unit_code = @business_unit_code
	AND dgp.group_page_name_resource_key is null
	AND NOT EXISTS	(
						SELECT * FROM stage.stg_group_page_person sgp
						WHERE sgp.group_is_custom = 0
							AND sgp.group_page_name_resource_key is null
							AND sgp.group_name COLLATE Latin1_General_CS_AS = dgp.group_name COLLATE Latin1_General_CS_AS
							AND sgp.business_unit_code = dgp.business_unit_code
					)

-- Delete custom group pages from dimension that does not exist in stage.
DELETE FROM mart.dim_group_page
FROM mart.dim_group_page dgp
WHERE dgp.group_is_custom = 1
	AND dgp.business_unit_code = @business_unit_code
	AND NOT EXISTS	(
						SELECT * FROM stage.stg_group_page_person sgp
						WHERE sgp.group_is_custom = 1
							AND sgp.group_page_code = dgp.group_page_code
							AND sgp.group_code = dgp.group_code
							AND sgp.business_unit_code = dgp.business_unit_code
					)

					
-- Update non custom group pages (excluding Note)
UPDATE 
	mart.dim_group_page 
SET group_page_name			= sgp.group_page_name, 
	group_name				= sgp.group_name,
	business_unit_name		= sgp.business_unit_name,
	datasource_update_date	= sgp.update_date
FROM 
	mart.dim_group_page dgp
INNER JOIN 
	stage.stg_group_page_person sgp
ON 
	dgp.group_page_name_resource_key = sgp.group_page_name_resource_key
		AND dgp.group_code = sgp.group_code
		AND dgp.business_unit_code = sgp.business_unit_code
WHERE dgp.group_is_custom = 0
	AND dgp.group_page_name_resource_key <> @note_resource_name
	
-- Update Note group page
UPDATE 
	mart.dim_group_page 
SET group_page_name			= sgp.group_page_name, 
	business_unit_name		= sgp.business_unit_name,
	datasource_update_date	= sgp.update_date
FROM 
	mart.dim_group_page dgp
INNER JOIN 
	stage.stg_group_page_person sgp
ON 
	dgp.group_page_name_resource_key = sgp.group_page_name_resource_key
		AND sgp.group_name COLLATE Latin1_General_CS_AS = dgp.group_name COLLATE Latin1_General_CS_AS
		AND dgp.business_unit_code = sgp.business_unit_code
WHERE dgp.group_is_custom = 0
	AND dgp.group_page_name_resource_key = @note_resource_name

-- Update optional columns group pages
UPDATE 
	mart.dim_group_page 
SET group_page_name			= sgp.group_page_name, 
	business_unit_name		= sgp.business_unit_name,
	datasource_update_date	= sgp.update_date
FROM 
	mart.dim_group_page dgp
INNER JOIN 
	stage.stg_group_page_person sgp
ON 
	sgp.group_page_code = dgp.group_page_code	
WHERE dgp.group_is_custom = 0
	AND dgp.group_page_name_resource_key is null


-- Update custom group pages
UPDATE 
	mart.dim_group_page 
SET group_page_name		= sgp.group_page_name, 
	group_name			= sgp.group_name,
	business_unit_name	= sgp.business_unit_name,
	datasource_update_date	= sgp.update_date
FROM 
	mart.dim_group_page dgp
INNER JOIN 
	stage.stg_group_page_person sgp
ON 
	dgp.group_page_code = sgp.group_page_code
		AND dgp.group_code = sgp.group_code
		AND dgp.business_unit_code = sgp.business_unit_code
WHERE dgp.group_is_custom = 1



-- Insert new non custom group pages (excluding Note)
DECLARE NoneCustomCursorExcludingNote CURSOR FOR
SELECT DISTINCT
	sgp.group_page_code,
	sgp.group_page_name,
	sgp.group_page_name_resource_key,
	sgp.group_code,
	sgp.group_name,
	sgp.datasource_id
FROM stage.stg_group_page_person sgp
WHERE sgp.group_is_custom = 0
	AND sgp.business_unit_code = @business_unit_code
	AND sgp.group_page_name_resource_key <> @note_resource_name
	AND NOT EXISTS	(
						SELECT * FROM mart.dim_group_page dgp
						WHERE sgp.group_is_custom = 0
							AND sgp.group_page_name_resource_key = dgp.group_page_name_resource_key
							AND sgp.group_code = dgp.group_code
							AND sgp.business_unit_code = dgp.business_unit_code
					)
OPEN NoneCustomCursorExcludingNote
FETCH NEXT FROM NoneCustomCursorExcludingNote INTO @group_page_code,@group_page_name,@group_page_name_resource_key,@group_code,@group_name,@datasource_id
WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @counter = ISNULL(MAX(group_id), 0) FROM mart.dim_group_page
	SET @counter = @counter + 1
	
	-- Insert row where the group page already exists
	INSERT INTO mart.dim_group_page
	SELECT DISTINCT
		dgp.group_page_code,
		@group_page_name,
		@group_page_name_resource_key,
		@counter,
		@group_code,
		@group_name,
		0,
		bu.business_unit_id,
		@business_unit_code,
		bu.business_unit_name,
		@datasource_id,
		GETDATE(),
		GETDATE()
	FROM 
		mart.dim_group_page dgp
	INNER JOIN
		mart.dim_business_unit bu
	ON
		dgp.business_unit_code = bu.business_unit_code
	WHERE dgp.group_is_custom = 0
		AND dgp.group_page_name_resource_key = @group_page_name_resource_key
		AND dgp.business_unit_code = @business_unit_code
		AND dgp.group_page_name_resource_key <> @note_resource_name
	
	IF @@ROWCOUNT = 0
	BEGIN
		-- Insert row where the group page NOT already exists
		INSERT INTO mart.dim_group_page
		SELECT
			@group_page_code,
			@group_page_name,
			@group_page_name_resource_key,
			@counter,
			@group_code,
			@group_name,
			0,
			business_unit_id,
			@business_unit_code,
			business_unit_name,
			@datasource_id,
			GETDATE(),
			GETDATE()
		FROM mart.dim_business_unit
		WHERE business_unit_code = @business_unit_code
	END
	
	FETCH NEXT FROM NoneCustomCursorExcludingNote INTO @group_page_code,@group_page_name,@group_page_name_resource_key,@group_code,@group_name,@datasource_id
END
CLOSE NoneCustomCursorExcludingNote
DEALLOCATE NoneCustomCursorExcludingNote


-- Insert new Note group pages
DECLARE NoteCursor CURSOR FOR
SELECT DISTINCT
	sgp.group_page_code,
	sgp.group_page_name,
	sgp.group_page_name_resource_key,
	sgp.group_code,
	sgp.group_name COLLATE Latin1_General_CS_AS,
	sgp.datasource_id
FROM stage.stg_group_page_person sgp
WHERE sgp.group_is_custom = 0
	AND sgp.business_unit_code = @business_unit_code
	AND sgp.group_page_name_resource_key = @note_resource_name
	AND NOT EXISTS	(
						SELECT * FROM mart.dim_group_page dgp
						WHERE sgp.group_is_custom = 0
							AND sgp.group_page_name_resource_key = dgp.group_page_name_resource_key
							AND sgp.group_name COLLATE Latin1_General_CS_AS = dgp.group_name COLLATE Latin1_General_CS_AS 
							AND sgp.business_unit_code = dgp.business_unit_code
					)
OPEN NoteCursor
FETCH NEXT FROM NoteCursor INTO @group_page_code,@group_page_name,@group_page_name_resource_key,@group_code,@group_name,@datasource_id
WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @counter = ISNULL(MAX(group_id), 0) FROM mart.dim_group_page
	SET @counter = @counter + 1
	
	-- Insert row where the group page already exists
	INSERT INTO mart.dim_group_page
	SELECT DISTINCT
		dgp.group_page_code,
		@group_page_name,
		@group_page_name_resource_key,
		@counter,
		@group_code,
		@group_name,
		0,
		bu.business_unit_id,
		@business_unit_code,
		bu.business_unit_name,
		@datasource_id,
		GETDATE(),
		GETDATE()
	FROM 
		mart.dim_group_page dgp
	INNER JOIN
		mart.dim_business_unit bu
	ON
		dgp.business_unit_code = bu.business_unit_code
	WHERE dgp.group_is_custom = 0
		AND dgp.group_page_name_resource_key = @group_page_name_resource_key
		AND dgp.business_unit_code = @business_unit_code
		AND dgp.group_page_name_resource_key = @note_resource_name
	
	IF @@ROWCOUNT = 0
	BEGIN
		-- Insert row where the group page NOT already exists
		INSERT INTO mart.dim_group_page
		SELECT
			@group_page_code,
			@group_page_name,
			@group_page_name_resource_key,
			@counter,
			@group_code,
			@group_name,
			0,
			business_unit_id,
			@business_unit_code,
			business_unit_name,
			@datasource_id,
			GETDATE(),
			GETDATE()
		FROM mart.dim_business_unit
		WHERE business_unit_code = @business_unit_code
	END
	
	FETCH NEXT FROM NoteCursor INTO @group_page_code,@group_page_name,@group_page_name_resource_key,@group_code,@group_name,@datasource_id
END
CLOSE NoteCursor
DEALLOCATE NoteCursor


-- Insert new custom group pages

DECLARE CustomCursor CURSOR FOR
SELECT DISTINCT
	sgp.group_page_code,
	sgp.group_page_name,
	sgp.group_page_name_resource_key,
	sgp.group_code,
	sgp.group_name,
	sgp.datasource_id
FROM stage.stg_group_page_person sgp
WHERE sgp.group_is_custom = 1
	AND sgp.business_unit_code = @business_unit_code
	AND NOT EXISTS	(
						SELECT * FROM mart.dim_group_page dgp
						WHERE sgp.group_is_custom = 1
							AND sgp.group_page_code = dgp.group_page_code
							AND sgp.group_code = dgp.group_code
							AND sgp.business_unit_code = dgp.business_unit_code
					)
	
OPEN CustomCursor
FETCH NEXT FROM CustomCursor INTO @group_page_code,@group_page_name,@group_page_name_resource_key,@group_code,@group_name,@datasource_id
WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @counter = ISNULL(MAX(group_id), 0) FROM mart.dim_group_page
	SET @counter = @counter + 1
	
	INSERT INTO mart.dim_group_page
	SELECT
		@group_page_code,
		@group_page_name,
		@group_page_name_resource_key,
		@counter,
		@group_code,
		@group_name,
		1,
		business_unit_id,
		@business_unit_code,
		business_unit_name,
		@datasource_id,
		GETDATE(),
		GETDATE()
	FROM mart.dim_business_unit
	WHERE business_unit_code = @business_unit_code
	
	FETCH NEXT FROM CustomCursor INTO @group_page_code,@group_page_name,@group_page_name_resource_key,@group_code,@group_name,@datasource_id
END
CLOSE CustomCursor
DEALLOCATE CustomCursor

-- Insert new optional column group pages
DECLARE OptionalColumnCursor CURSOR FOR
SELECT DISTINCT
	sgp.group_page_code,
	sgp.group_page_name,
	sgp.group_page_name_resource_key,
	sgp.group_code,
	sgp.group_name COLLATE Latin1_General_CS_AS,
	sgp.datasource_id
FROM stage.stg_group_page_person sgp
WHERE sgp.group_is_custom = 0
	AND sgp.business_unit_code = @business_unit_code
	AND sgp.group_page_name_resource_key is null
	AND NOT EXISTS	(
						SELECT * FROM mart.dim_group_page dgp
						WHERE sgp.group_is_custom = 0
							AND sgp.group_page_name_resource_key is null
							AND sgp.group_name COLLATE Latin1_General_CS_AS = dgp.group_name COLLATE Latin1_General_CS_AS 
							AND sgp.business_unit_code = dgp.business_unit_code
					)
OPEN OptionalColumnCursor
FETCH NEXT FROM OptionalColumnCursor INTO @group_page_code,@group_page_name,@group_page_name_resource_key,@group_code,@group_name,@datasource_id
WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @counter = ISNULL(MAX(group_id), 0) FROM mart.dim_group_page
	SET @counter = @counter + 1
	
	-- Insert row where the group page NOT already exists
	INSERT INTO mart.dim_group_page
	SELECT
		@group_page_code,
		@group_page_name,
		@group_page_name_resource_key,
		@counter,
		@group_code,
		@group_name,
		0,
		business_unit_id,
		@business_unit_code,
		business_unit_name,
		@datasource_id,
		GETDATE(),
		GETDATE()
	FROM mart.dim_business_unit
	WHERE business_unit_code = @business_unit_code
	
	FETCH NEXT FROM OptionalColumnCursor INTO @group_page_code,@group_page_name,@group_page_name_resource_key,@group_code,@group_name,@datasource_id
END
CLOSE OptionalColumnCursor
DEALLOCATE OptionalColumnCursor

	
GO