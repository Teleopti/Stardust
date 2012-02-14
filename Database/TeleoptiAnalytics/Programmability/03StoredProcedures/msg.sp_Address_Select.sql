/****** Object:  StoredProcedure [msg].[sp_Address_Select]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Address_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Address_Select]
GO


----------------------------------------------------------------------------
-- Select a single record from Address
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Address_Select]
	@AddressId int
AS

SELECT	AddressId,
	[Address],
	Port
FROM	Msg.Address
WHERE 	AddressId = @AddressId

GO
