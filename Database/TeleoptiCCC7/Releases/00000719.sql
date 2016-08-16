GO
/****** Object:  Table [dbo].[SiteOpenHour]    Script Date: 8/16/2016 9:38:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SiteOpenHours]') AND type in (N'U'))
begin
	drop table SiteOpenHours
end
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SiteOpenHour]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SiteOpenHour](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[WeekDay] [int] NOT NULL,
	[StartTime] [bigint] NOT NULL,
	[EndTime] [bigint] NOT NULL,
	[IsClosed] [bit] NOT NULL,
 CONSTRAINT [PK_SiteOpenHour] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SiteOpenHour_Person_UpdatedBy]') AND parent_object_id = OBJECT_ID(N'[dbo].[SiteOpenHour]'))
ALTER TABLE [dbo].[SiteOpenHour]  WITH CHECK ADD  CONSTRAINT [FK_SiteOpenHour_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SiteOpenHour_Person_UpdatedBy]') AND parent_object_id = OBJECT_ID(N'[dbo].[SiteOpenHour]'))
ALTER TABLE [dbo].[SiteOpenHour] CHECK CONSTRAINT [FK_SiteOpenHour_Person_UpdatedBy]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SiteOpenHours_Site]') AND parent_object_id = OBJECT_ID(N'[dbo].[SiteOpenHour]'))
ALTER TABLE [dbo].[SiteOpenHour]  WITH CHECK ADD  CONSTRAINT [FK_SiteOpenHours_Site] FOREIGN KEY([Parent])
REFERENCES [dbo].[Site] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SiteOpenHours_Site]') AND parent_object_id = OBJECT_ID(N'[dbo].[SiteOpenHour]'))
ALTER TABLE [dbo].[SiteOpenHour] CHECK CONSTRAINT [FK_SiteOpenHours_Site]
GO
