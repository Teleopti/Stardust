
----------------  
--Name: Roger Kratz
--Date: 2013-04-15
--Desc: Adding date for person assignment. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE dbo.PersonAssignment
ADD TheDate datetime
GO

update dbo.PersonAssignment
set TheDate ='1800-01-01'
GO

ALTER TABLE dbo.PersonAssignment
ALTER COLUMN TheDate datetime not null
GO

----------------  
--Name: Roger Kratz
--Date: 2013-04-15
--Desc: Adding date for person assignment audit table. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE Auditing.PersonAssignment_AUD
ADD TheDate datetime
GO

update Auditing.PersonAssignment_AUD
set TheDate ='1800-01-01'
GO

----------------  
--Name: Roger Kratz
--Date: 2013-05-15

---------------- MAIN TABLES --------------------------

--Add shiftcategory to personassignment
ALTER TABLE dbo.PersonAssignment
add ShiftCategory uniqueidentifier

ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO

ALTER TABLE [dbo].[PersonAssignment] CHECK CONSTRAINT [FK_PersonAssignment_ShiftCategory]
GO

--move data for ShiftCategory
UPDATE pa
SET pa.ShiftCategory = ms.ShiftCategory
FROM dbo.PersonAssignment pa
INNER JOIN dbo.MainShift ms
	ON pa.Id = ms.Id

--add mainshiftlayers from personassignment
ALTER TABLE [dbo].[MainShiftActivityLayer] DROP CONSTRAINT [FK_MainShiftActivityLayer_MainShift]
GO

ALTER TABLE [dbo].[MainShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayer_PersonAssignment] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAssignment] ([Id])
on delete cascade
GO

ALTER TABLE [dbo].[MainShiftActivityLayer] CHECK CONSTRAINT [FK_MainShiftActivityLayer_PersonAssignment]
GO

--drop mainshift table
DROP TABLE dbo.MainShift
GO

------------------ AUDIT TABLES ----------------------------
--Add shiftcategory to personassignment
ALTER TABLE auditing.PersonAssignment_AUD
add ShiftCategory uniqueidentifier
GO

--move data for ShiftCategory
UPDATE pa
SET pa.ShiftCategory = ms.ShiftCategory
FROM auditing.PersonAssignment_AUD pa
INNER JOIN auditing.MainShift_AUD ms
	ON pa.Id = ms.Id

--drop mainshift table
drop table auditing.mainshift_aud
GO