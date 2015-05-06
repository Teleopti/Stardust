IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[pm_user_check]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[pm_user_check]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		jonas n
-- Create date: 2010-06-24
-- Ola 2015-05-06 pm_user table has changed to contain the access_level
-- Description:	Truncate table mart.pm_user
--  [mart].[pm_user_check] '10957AD5-5489-48E0-959A-9B5E015B2B5C'
-- [mart].[pm_user_check] 'CCB9C388-F65B-483F-94CA-9B5E015B2564'
-- =============================================
CREATE PROCEDURE [mart].[pm_user_check] 
@person_id uniqueidentifier
WITH EXECUTE AS OWNER
AS
BEGIN

IF EXISTS(SELECT *
    FROM mart.pm_user
    WHERE [person_id] = @person_id)
BEGIN
	SELECT access_level FROM mart.pm_user
    WHERE [person_id] = @person_id
	return
   END
    SELECT 0
END

GO


