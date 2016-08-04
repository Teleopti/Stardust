GO
/****** Object:  Table [dbo].[SiteOpenHours]    Script Date: 8/4/2016 10:33:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SiteOpenHours]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SiteOpenHours](
	[Parent] [uniqueidentifier] NOT NULL,
	[Weekday] [int] NOT NULL,
	[StartTime] [bigint] NOT NULL,
	[EndTime] [bigint] NOT NULL,
 CONSTRAINT [PK_SiteOpenHours] PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[Weekday] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SiteOpenHours_Site]') AND parent_object_id = OBJECT_ID(N'[dbo].[SiteOpenHours]'))
ALTER TABLE [dbo].[SiteOpenHours]  WITH CHECK ADD  CONSTRAINT [FK_SiteOpenHours_Site] FOREIGN KEY([Parent])
REFERENCES [dbo].[Site] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SiteOpenHours_Site]') AND parent_object_id = OBJECT_ID(N'[dbo].[SiteOpenHours]'))
ALTER TABLE [dbo].[SiteOpenHours] CHECK CONSTRAINT [FK_SiteOpenHours_Site]
GO
