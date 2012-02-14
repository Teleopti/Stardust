IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[SplitStringInt]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[SplitStringInt]
GO



-- SELECT * FROM SplitStringInt('1,2,3,4,5,6,7,8,9')

CREATE   FUNCTION [mart].[SplitStringInt]
-- Takes an input string with strings separated by commas and
-- inserts the result into a field called id in a given table 
-- with name @table_name
--
-- Created: 990322 by viktor.edlund@advisorconsulting.se
-- Last changed: 990513 by viktor.edlund@advisorconsulting.se
-- Last changed: 990819 by Micke
-- Omgjord till en funktion Ola 2004-11-09
-- returnerar en tabell istället
(@string_string varchar(max))
RETURNS @strings TABLE (id int NOT NULL)
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

