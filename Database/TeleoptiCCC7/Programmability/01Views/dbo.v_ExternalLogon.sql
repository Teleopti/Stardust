--Needed for clustered index on view
SET NUMERIC_ROUNDABORT OFF;
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT,
    QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_ExternalLogon]'))
DROP VIEW [dbo].[v_ExternalLogon]
GO

CREATE VIEW [dbo].[v_ExternalLogon]
WITH SCHEMABINDING
AS

SELECT
ecl.PersonPeriod,
el.AcdLogOnOriginalId,
el.DataSourceId
FROM [dbo].[ExternalLogOnCollection] ecl
INNER JOIN [dbo].[ExternalLogOn] el
	ON ecl.ExternalLogOn = el.Id
GO

CREATE UNIQUE CLUSTERED INDEX CIX_v_ExternalLogon
    ON dbo.v_ExternalLogon (PersonPeriod,DataSourceId,AcdLogOnOriginalId)
GO

