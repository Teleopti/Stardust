/****** Object:  StoredProcedure [msg].[sp_ConcurrentUsers_Select]    Script Date: 03/08/2010 10:30:42 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_ConcurrentUsers_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_ConcurrentUsers_Select]

/****** Object:  StoredProcedure [msg].[sp_ConcurrentUsers_Select]    Script Date: 03/08/2010 10:30:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [msg].[sp_ConcurrentUsers_Select]
@Address nvarchar(30)
AS
 
SELECT IpAddress, COUNT(*) AS NumberOfConcurrentUsers 
FROM MSG.Subscriber 
WHERE IpAddress = @Address
GROUP BY IpAddress
