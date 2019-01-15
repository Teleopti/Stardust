----------------  
--Name: Mingdi
--Date: 2019-01-15
--Desc: Add constraint with site and businessUnit
---------------- 

ALTER TABLE [dbo].[SiteBankHolidayCalendar] ADD  CONSTRAINT [UQ_Site_BusinessUnit] UNIQUE NONCLUSTERED 
(
	[Site] ASC,
	[BusinessUnit] ASC
)
GO