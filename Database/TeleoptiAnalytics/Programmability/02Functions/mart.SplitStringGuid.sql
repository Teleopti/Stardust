IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[SplitStringGuid]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[SplitStringGuid]
GO



-- SELECT * FROM mart.SplitStringGuid('475382D4-CA10-47DA-880D-9B5E015B2569,E34730FD-9B4D-4572-961A-9B5E015B2564,DDDF45A9-BDC4-4F0E-88B3-9B5E015B257C,893F41D5-A472-4C47-8EE8-9B5E015B2572')

CREATE   FUNCTION [mart].[SplitStringGuid]
-- Takes an input string with strings separated by commas and
-- returns a table containing ids
--
-- Created: 110117 by Mattias Engblom
(@string_string varchar(max))
RETURNS @strings TABLE (id uniqueidentifier NOT NULL)
As
BEGIN 

 DECLARE @pos int
 DECLARE @string varchar(50)
 DECLARE @insert_text varchar(100)
 -- Exit if an empty string is given 
 IF @string_string = '' BEGIN
  RETURN 
 END 
 -- For simplicty concatenate , at the end of the string
 SELECT @string_string = @string_string + ','
 -- Ensure that @pos <> 0  
 SELECT @pos = CHARINDEX(',', @string_string )
 WHILE @pos <> 0 BEGIN
  -- Get the position of the first ,
  SELECT @pos = CHARINDEX(',', @string_string )
  
  -- Exit?
  IF @pos = 0 OR @pos = 1 OR @string_string = ','
   return
  -- Extract the substring
  SELECT @string = SUBSTRING(@string_string,1,@pos-1)
  -- Skip leading blanks
  SELECT @string = LTRIM(@string)
  -- Extract everything except the string
  SELECT @string_string = STUFF (@string_string,1,@pos,'')
  -- Insert the string into the return table
	INSERT INTO @strings
	SELECT @string
  
 END

RETURN

END




GO

