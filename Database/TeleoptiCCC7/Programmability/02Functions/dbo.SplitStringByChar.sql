IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitStringByChar]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[SplitStringByChar]
GO

CREATE FUNCTION [dbo].[SplitStringByChar]
(
	@string_string varchar(8000), 
	@a_char char
)
RETURNS @strings TABLE 
(
	string varchar(100) NOT NULL
)
AS
BEGIN 

	DECLARE @pos int
	DECLARE @string varchar(50)
	DECLARE @insert_text varchar(100)
	-- Exit if an empty string is given 
	IF @string_string = '' BEGIN
		RETURN 
	END 
	-- For simplicty concatenate , at the end of the string
	SELECT @string_string = @string_string + @a_char
	-- Ensure that @pos <> 0  
	SELECT @pos = CHARINDEX(@a_char, @string_string )
	WHILE @pos <> 0 BEGIN
	-- Get the position of the first ,
	SELECT @pos = CHARINDEX(@a_char, @string_string )
  
	-- Exit?
	IF @pos = 0 OR @pos = 1 OR @string_string = @a_char
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

