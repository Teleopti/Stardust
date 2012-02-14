/****** Object:  StoredProcedure [msg].[sp_Address_Update]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Address_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Address_Update]
GO


----------------------------------------------------------------------------
-- Update a single record in Address
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Address_Update]
	@AddressId int,
	@Address nvarchar(255),
	@Port int
AS

UPDATE	Msg.Address
SET	[Address] = @Address,
	Port = @Port
WHERE 	AddressId = @AddressId

GO

