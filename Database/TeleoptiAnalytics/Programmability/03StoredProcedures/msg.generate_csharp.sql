IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[generate_csharp]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[generate_csharp]
GO



CREATE       PROCEDURE [msg].[generate_csharp] 
(
@objName nvarchar(100)
)
AS
/*
___________________________________________________________________
Name:  		CS SPROC Builder
Version: 	1
Date:    	10/09/2004
Author:  	Paul McKenzie
Description: 	Call this stored procedue passing the name of your 
  database object that you wish to insert/update
  from .NET (C#) and the code returns code to copy
  and paste into your application.  This version is
  for use with "Microsoft Data Application Block".
  
Version: 	1.1
Date:	 	17/02/2006
Author:	 	Paul McKenzie
Description:	a) Updated to include 'UniqueIdentifier' Data Type
		b) Support for 'ParameterDirection.Output'

*/
SET NOCOUNT ON
DECLARE @parameterCount int
DECLARE @errMsg varchar(100)
DECLARE @parameterAt varchar(1)
DECLARE @connName varchar(100)
DECLARE @outputValues varchar(100)
--Change the following variable to the name of your connection instance
SET @connName='conn.Connection'
SET @parameterAt=''
SET @outputValues=''
SELECT 
 	dbo.sysobjects.name AS ObjName, 
 	dbo.sysobjects.xtype AS ObjType,
 	dbo.syscolumns.name AS ColName, 
 	dbo.syscolumns.colorder AS ColOrder, 
 	dbo.syscolumns.length AS ColLen, 
 	dbo.syscolumns.colstat AS ColKey, 
 	dbo.syscolumns.isoutparam AS ColIsOut,
 	dbo.systypes.xtype
INTO #t_obj
FROM         
 	dbo.syscolumns INNER JOIN
 	dbo.sysobjects ON dbo.syscolumns.id = dbo.sysobjects.id INNER JOIN
 	dbo.systypes ON dbo.syscolumns.xtype = dbo.systypes.xtype
WHERE     
 	(dbo.sysobjects.name = @objName) 
 	AND 
 	(dbo.systypes.status <> 1) 
ORDER BY 
 	dbo.sysobjects.name, 
 	dbo.syscolumns.colorder

SET @parameterCount=(SELECT count(*) FROM #t_obj)
IF(@parameterCount<1) SET @errMsg='No Parameters/Fields found for ' + @objName
IF(@errMsg is null)
	BEGIN
  		PRINT 'try'
  		PRINT '   {'
  		PRINT '   SqlParameter[] paramsToStore = new SqlParameter[' + cast(@parameterCount as varchar) + '];'
  		PRINT ''
  
  		DECLARE @source_name nvarchar,@source_type varchar,
    			@col_name nvarchar(100),@col_order int,@col_type varchar(20),
    			@col_len int,@col_key int,@col_xtype int,@col_redef varchar(20), @col_isout tinyint
 
  		DECLARE cur CURSOR FOR
  		SELECT * FROM #t_obj
  		OPEN cur
  		-- Perform the first fetch.
  		FETCH NEXT FROM cur INTO @source_name,@source_type,@col_name,@col_order,@col_len,@col_key,@col_isout,@col_xtype
 
  			if(@source_type=N'U') SET @parameterAt='@'
  			-- Check @@FETCH_STATUS to see if there are any more rows to fetch.
  			WHILE @@FETCH_STATUS = 0
  				BEGIN
   				SET @col_redef=(SELECT CASE @col_xtype
					WHEN 34 THEN 'Image'
					WHEN 35 THEN 'Text'
					WHEN 36 THEN 'UniqueIdentifier'
					WHEN 48 THEN 'TinyInt'
					WHEN 52 THEN 'SmallInt'
					WHEN 56 THEN 'Int'
					WHEN 58 THEN 'SmallDateTime'
					WHEN 59 THEN 'Real'
					WHEN 60 THEN 'Money'
					WHEN 61 THEN 'DateTime'
					WHEN 62 THEN 'Float'
					WHEN 99 THEN 'NText'
					WHEN 104 THEN 'Bit'
					WHEN 106 THEN 'Decimal'
					WHEN 122 THEN 'SmallMoney'
					WHEN 127 THEN 'BigInt'
					WHEN 165 THEN 'VarBinary'
					WHEN 167 THEN 'VarChar'
					WHEN 173 THEN 'Binary'
					WHEN 175 THEN 'Char'
					WHEN 231 THEN 'NVarChar'
					WHEN 239 THEN 'NChar'
					ELSE '!MISSING'
					END AS C) 

				--Write out the parameter
				PRINT '   paramsToStore[' + cast(@col_order-1 as varchar) 
				    + '] = new SqlParameter("' + @parameterAt + @col_name
				    + '", SqlDbType.' + @col_redef
				    + ');'

				--Write out the parameter direction it is output
				IF(@col_isout=1)
					BEGIN
						PRINT '   paramsToStore['+ cast(@col_order-1 as varchar) +'].Direction=ParameterDirection.Output;'
						SET @outputValues=@outputValues+'   ?=paramsToStore['+ cast(@col_order-1 as varchar) +'].Value;'
					END
					ELSE
					BEGIN
						--Write out the parameter value line
   						PRINT '   paramsToStore['+ cast(@col_order-1 as varchar) + '].Value = ?;'
					END
				--If the type is a string then output the size declaration
				IF(@col_xtype=231)OR(@col_xtype=167)OR(@col_xtype=175)OR(@col_xtype=99)OR(@col_xtype=35)
					BEGIN
						PRINT '   paramsToStore[' + cast(@col_order-1 as varchar) + '].Size=' + cast(@col_len as varchar) + ';'
					END

				 -- This is executed as long as the previous fetch succeeds.
      			FETCH NEXT FROM cur INTO @source_name,@source_type,@col_name,@col_order, @col_len,@col_key,@col_isout,@col_xtype 
  	END
  PRINT ''
  PRINT '   SqlHelper.ExecuteNonQuery(' + @connName + ', CommandType.StoredProcedure,"' + @objName + '", paramsToStore);'
  PRINT @outputValues
  PRINT '   }'
  PRINT 'catch(Exception excp)'
  PRINT '   {'
  PRINT '   }'
  PRINT 'finally'
  PRINT '   {'
  PRINT '   ' + @connName + '.Dispose();'
  PRINT '   ' + @connName + '.Close();'
  PRINT '   }'  
  CLOSE cur
  DEALLOCATE cur
 END
if(LEN(@errMsg)>0) PRINT @errMsg
DROP TABLE #t_obj
SET NOCOUNT ON


GO

