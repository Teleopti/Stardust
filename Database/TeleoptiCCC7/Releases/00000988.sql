DROP INDEX [CIX_PreferenceDay_DatePersonBU] ON [dbo].[PreferenceDay]
GO

CREATE CLUSTERED INDEX [CIX_PreferenceDay_DatePersonBU] ON [dbo].[PreferenceDay]
(
	[Person] ASC,
	[RestrictionDate] ASC,
	[BusinessUnit] ASC
)
GO