ALTER TABLE [dbo].[StudentAvailabilityDay]
ADD CONSTRAINT [PK_StudentAvailabilityDay_Person_RestrictionDate] UNIQUE (Person, RestrictionDate)
GO