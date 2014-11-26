----------------  
--Name: Xianwei Shen
--Date: 2014-11-20
--Desc: Add new table for adherence details
---------------- 

DROP TABLE [ReadModel].[AdherenceDetails];
GO
CREATE TABLE [ReadModel].[AdherenceDetails](
	[PersonId] [uniqueidentifier] NOT NULL,
	[BelongsToDate] [smalldatetime] NOT NULL,
	[Model] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_AdherenceDetails] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC,
	[BelongsToDate] ASC
)
)
GO