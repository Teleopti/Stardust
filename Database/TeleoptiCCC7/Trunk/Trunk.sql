
/*
Make sure that all persons in the database has write protection info. This is created automatically in the domain.
*/

INSERT INTO [dbo].[PersonWriteProtectionInfo] (Id,CreatedBy,UpdatedBy,CreatedOn,UpdatedOn,PersonWriteProtectedDate) SELECT p.Id,p.CreatedBy,p.UpdatedBy,p.CreatedOn,p.UpdatedOn,null FROM [dbo].[Person] p WHERE p.Id NOT IN (SELECT id FROM [dbo].[PersonWriteProtectionInfo])
GO

/*
We have changed the lowest possible resolution to one hour to avoid issues with daylight savings.
*/

UPDATE dbo.Skill SET DefaultResolution = 60 WHERE DefaultResolution > 60
GO

-- =============================================
-- Author:		Ola
-- Create date: 2012-02-28
-- Description:	New LicenseStatus table
-- =============================================
CREATE TABLE [dbo].[LicenseStatus](
	[Id] [uniqueidentifier] NOT NULL,
	[XmlString] [nvarchar](4000) NOT NULL,
	CONSTRAINT PK_LicenseStatus PRIMARY KEY CLUSTERED (Id))
----------------  
--Name: David Jonsson
--Date: 2012-03-23
--Desc: #18738 - very slow to fetch request from MyTimeWeb
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShiftTradeSwapDetail]') AND name = N'IX_ShiftTradeSwapDetail')
CREATE NONCLUSTERED INDEX IX_ShiftTradeSwapDetail_Parent
ON [dbo].[ShiftTradeSwapDetail] ([Parent])
INCLUDE ([PersonFrom],[PersonTo])
GO


