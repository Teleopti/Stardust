/****** Object:  StoredProcedure [msg].[sp_Address_Delete]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Address_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Address_Delete]
GO

----------------------------------------------------------------------------
-- Delete a single record from Address
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Address_Delete]
	@AddressId int
AS

DELETE	Msg.Address
WHERE 	AddressId = @AddressId

GO

