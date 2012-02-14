/****** Object:  StoredProcedure [msg].[sp_Address_Insert]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Address_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Address_Insert]
GO

----------------------------------------------------------------------------
-- Insert a single record into Address
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Address_Insert]
	@AddressId int OUTPUT,
	@Address nvarchar(255),
	@Port int
AS

IF(@AddressId = 0)
	BEGIN
		DECLARE @RowsInTable INT 
		SET @RowsInTable = (SELECT COUNT(*) FROM [msg].[Address]) 
		IF(@RowsInTable = 0)
			SET @AddressId = 1
		ELSE
			SET @AddressId = (SELECT MAX(AddressId) + 1 FROM [msg].[Address]) 
		INSERT [msg].[Address](AddressId, [Address], Port)
		VALUES (@AddressId, @Address, @Port)
	END
ELSE
UPDATE	[msg].[Address]
SET	AddressId = @AddressId,
	Port = @Port
WHERE 	AddressId = @AddressId

GO

