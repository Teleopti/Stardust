IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_sort_by_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_sort_by_get]
GO



-- =============================================
-- Author:		<KJ>
-- Create date: <2008-07-02>
-- Update date: 2008-09-10
--				20080910 Added parameter @bu_id KJ
--				20080924 Changed language translation handling if @language is missing in language_translation(then return english) KJ
--				20090211 Added new mart schema KJ
--				20090812 Changed fetch of texts from the new table Ola
--				20090818 Corrected the new fetch of texts from the new table Ola
--				20110808 Corrected the fetch and (re)added shift start and shift end 
-- Description:	<Gets sorting for report Agent Schedule Adherence>
-- =============================================
--exec report_control_sort_by_get 13,'C04803E2-8D6F-4936-9A90-9B2000148778',1053,'4AD43E49-B233-4D03-A813-9B2000102EBE'
-- mart.report_control_sort_by_get 13, '10957ad5-5489-48e0-959a-9b5e015b2b5c', 1033, '928dd0bc-bf40-412e-b970-9b5e015aadea'

CREATE PROCEDURE [mart].[report_control_sort_by_get]
@report_id int,
@person_code uniqueidentifier,
@language_id int,
@bu_id uniqueidentifier
AS
BEGIN

SET NOCOUNT ON;

CREATE TABLE #result(id int, name nvarchar(50))

IF exists(SELECT * FROM mart.language_translation WHERE language_id = @language_id AND term_english = 'First Name')	
INSERT #result
SELECT
	id		= 1,
	name = ISNULL(term_language, 'First Name')
FROM
	mart.language_translation l WHERE
	language_id = @language_id
AND
	term_english = 'First Name'	
ELSE
INSERT #result 
SELECT
	id		= 1,
	name = 'First Name'
	
IF exists(SELECT * FROM mart.language_translation WHERE language_id = @language_id AND term_english = 'Last Name')	
INSERT #result
	SELECT
	id		= 2,
	name = ISNULL(term_language, term_english)
FROM
	mart.language_translation t
WHERE
	language_id = @language_id
AND
	term_english = 'Last Name'	
ELSE
INSERT #result 
SELECT
	id		= 2,
	name = 'Last Name'

IF exists(SELECT * FROM mart.language_translation WHERE language_id = @language_id AND term_english = 'Shift Start Time')
INSERT #result
	SELECT
	id		= 3,
	name	= term_language
FROM
	language_translation l
WHERE
	language_id = @language_id	AND
	term_english = 'Shift Start Time'	
ELSE
INSERT #result 
SELECT
	id		= 3,
	name = 'Shift Start Time'
		
IF exists(SELECT * FROM mart.language_translation WHERE language_id = @language_id AND term_english = 'Adherence')
INSERT #result
	SELECT
	id		= 4,
	name = ISNULL(term_language, term_english)
FROM
	mart.language_translation l 
WHERE
	language_id = @language_id
AND
	term_english = 'Adherence'	
ELSE
INSERT #result 
SELECT
	id		= 4,
	name = 'Adherence'

IF exists(SELECT * FROM mart.language_translation WHERE language_id = @language_id AND term_english = 'Shift End Time')
INSERT #result
	SELECT
	id		= 5,
	name	= term_language
FROM
	language_translation l
WHERE
	language_id = @language_id	AND
	term_english = 'Shift End Time'	
ELSE
INSERT #result 
SELECT
	id		= 5,
	name = 'Shift End Time'

END
	
SELECT id,name FROM #result ORDER BY id
/*Sortering 1=Förnamn,2=Efternamn,3=Shift_start,4=Adherence,5=ShiftEnd*/


