IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TempLic]') AND type in (N'U'))
DROP TABLE [dbo].[TempLic]

CREATE TABLE TempLic(XmlCol xml);

INSERT TempLic
  SELECT CONVERT(xml, BulkColumn, 2) FROM 
    OPENROWSET(Bulk '$(LicFile)', SINGLE_BLOB) [rowsetresults]

DELETE FROM License

--re-init
DECLARE	@version	AS int
DECLARE @newid		AS uniqueidentifier
DECLARE @PersonGUID AS uniqueidentifier
DECLARE @now		AS datetime
DECLARE @LicString	AS nvarchar(4000)

SET @version = 1
SET @newid	= newid()
SELECT top 1 @PersonGUID = Id FROM dbo.person
SET @now				= getdate()

SELECT @LicString = CAST(XmlCol as varchar(4000)) FROM TempLic

--Different DDL on different versions -> some installation tests are unaware of the repo/version they are run
if exists (select 1 from sys.columns
where object_name(object_id) = 'License'
and name in ('CreatedBy','CreatedOn'))
BEGIN
	exec sp_executesql N'INSERT INTO dbo.License (Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, XmlString, Id) VALUES (@p0, @p1, @p1, @p2, @p2, @p3, @p4)',N'@p0 int,@p1 uniqueidentifier,@p2 datetime,@p3 nvarchar(4000),@p4 uniqueidentifier',
	@p0=1,
	@p1=@PersonGUID,
	@p2=@now,
	@p3=@LicString,
	@p4=@newid
END
ELSE
BEGIN
	exec sp_executesql N'INSERT INTO dbo.License (Version, UpdatedBy, UpdatedOn, XmlString, Id) VALUES (@p0, @p1, @p2, @p3, @p4)',N'@p0 int,@p1 uniqueidentifier,@p2 datetime,@p3 nvarchar(4000),@p4 uniqueidentifier',
	@p0=1,
	@p1=@PersonGUID,
	@p2=@now,
	@p3=@LicString,
	@p4=@newid
END
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TempLic]') AND type in (N'U'))
DROP TABLE [dbo].[TempLic]

GO
