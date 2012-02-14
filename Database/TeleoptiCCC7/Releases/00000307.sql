/* 
Trunk initiated: 
2010-11-08 
08:26
By: TOPTINET\johanr 
On TELEOPTI565 
*/


ALTER TABLE dbo.SkillDataPeriod ADD
	ManualAgents float(53) NULL
ALTER TABLE dbo.TemplateSkillDataPeriod ADD
	ManualAgents float(53) NULL
ALTER TABLE dbo.Skill ADD
	UnderstaffingFor float(53) NOT NULL CONSTRAINT DF_Skill_UnderstaffingFor DEFAULT 1
ALTER TABLE dbo.Activity ADD
	PayrollCode nvarchar(20) NULL
ALTER TABLE dbo.DayOffTemplate ADD
	PayrollCode nvarchar(20) NULL

/* 
Add PayrollCode for PersonDayOff and mirrored AuditTrail.PersonDayOff.
2010-12-01 
10:52
By: TOPTINET\xianweis
On TELEOPTI563
*/

IF EXISTS (SELECT * FROM sysobjects WHERE name = 'tr_PersonDayOff_UD' AND [type] = 'TR')
BEGIN
	ALTER TABLE dbo.PersonDayOff DISABLE trigger tr_PersonDayOff_UD
END

IF EXISTS (SELECT * FROM sysobjects WHERE name = 'tr_PersonDayOff_I' AND [type] = 'TR')
BEGIN
	ALTER TABLE dbo.PersonDayOff DISABLE trigger tr_PersonDayOff_I
END

ALTER TABLE dbo.PersonDayOff ADD
       PayrollCode nvarchar(20) NULL
ALTER TABLE AuditTrail.PersonDayOff ADD
       PayrollCode nvarchar(20) NULL

IF EXISTS (SELECT * FROM sysobjects WHERE name = 'tr_PersonDayOff_UD' AND [type] = 'TR')
BEGIN
	ALTER TABLE dbo.PersonDayOff ENABLE trigger tr_PersonDayOff_UD
END

IF EXISTS (SELECT * FROM sysobjects WHERE name = 'tr_PersonDayOff_I' AND [type] = 'TR')
BEGIN
	ALTER TABLE dbo.PersonDayOff ENABLE trigger tr_PersonDayOff_I
END


/* 
Add date when templates are updated.
2010-12-02
10:52
By: TOPTINET\xianweis
On TELEOPTI563
*/
ALTER TABLE dbo.WorkloadDay ADD
	UpdatedDate datetime NOT NULL CONSTRAINT DF_WorkloadDay_UpdatedDate DEFAULT ('2001-01-01T00:00:00')
ALTER TABLE dbo.WorkloadDayTemplate ADD
	UpdatedDate datetime NOT NULL CONSTRAINT DF_WorkloadDayTemplate_UpdatedDate DEFAULT ('2001-01-01T00:00:00')
ALTER TABLE dbo.SkillDay ADD
	UpdatedDate datetime NOT NULL CONSTRAINT DF_SkillDay_UpdatedDate DEFAULT ('2001-01-01T00:00:00')
ALTER TABLE dbo.SkillDayTemplate ADD
	UpdatedDate datetime NOT NULL CONSTRAINT DF_SkillDayTemplate_UpdatedDate DEFAULT ('2001-01-01T00:00:00')
ALTER TABLE dbo.MultisiteDay ADD
	UpdatedDate datetime NOT NULL CONSTRAINT DF_MultisiteDay_UpdatedDate DEFAULT ('2001-01-01T00:00:00')
ALTER TABLE dbo.MultisiteDayTemplate ADD
	UpdatedDate datetime NOT NULL CONSTRAINT DF_MultisiteDayTemplate_UpdatedDate DEFAULT ('2001-01-01T00:00:00')
GO
/* 
Assign initial values to SkillDay, MultisiteDay and WorkloadDay
2010-12-02
10:52
By: TOPTINET\xianweis
On TELEOPTI563
*/
UPDATE SkillDay
   SET 
      SkillDay.UpdatedDate = SkillDay.UpdatedOn
GO
UPDATE MultisiteDay
   SET 
      MultisiteDay.UpdatedDate = MultisiteDay.UpdatedOn
GO
UPDATE WorkloadDay
SET WorkloadDay.UpdatedDate = WorkloadDayTemplate.CreatedDate
FROM WorkloadDay 
LEFT JOIN WorkloadDayTemplate on WorkloadDay.Workload = WorkloadDayTemplate.Parent 
		AND WorkloadDay.TemplateName = WorkloadDayTemplate.Name
		AND WorkloadDay.VersionNumber = WorkloadDayTemplate.VersionNumber
WHERE WorkloadDayTemplate.CreatedDate IS NOT NULL
GO
UPDATE dbo.WorkloadDayTemplate
   SET UpdatedDate = CreatedDate
WHERE CREATEDDATE IS NOT NULL
GO
UPDATE SkillDayTemplate
SET SkillDayTemplate.UpdatedDate = SkillDay.UpdatedOn
FROM SkillDayTemplate 
LEFT JOIN SkillDay on SkillDayTemplate.Parent = SkillDay.Skill
		AND SkillDayTemplate.Name = SkillDay.TemplateName
		AND SkillDayTemplate.VersionNumber = SkillDay.VersionNumber
WHERE SkillDay.UpdatedOn IS NOT NULL
GO
UPDATE MultisiteDayTemplate
SET MultisiteDayTemplate.UpdatedDate = MultisiteDay.UpdatedOn
FROM MultisiteDayTemplate 
LEFT JOIN MultisiteDay on MultisiteDayTemplate.Parent = MultisiteDay.Skill
		AND MultisiteDayTemplate.Name = MultisiteDay.TemplateName
		AND MultisiteDayTemplate.VersionNumber = MultisiteDay.VersionNumber
WHERE MultisiteDay.UpdatedOn IS NOT NULL
GO

----------------  
--Name: David Jonsson
--Date: 2010-12-03
--Desc: Bug #12662 + #12663. Dubplicates in OrderIndex exists for a shift
--		Note: this script is delivered on 306 as a "function" with NO MERGE
--			  It should and can be executed multiple times
--			  It's also added on root in the same manner
----------------  

SET NOCOUNT ON
-------------------
--Create table to hold error layers
-------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MainShiftActivityLayerFix306]') AND type in (N'U'))
BEGIN 
	CREATE TABLE dbo.MainShiftActivityLayerFix306
	(
		Batch int NOT NULL,
		UpdatedOn smalldatetime NOT NULL,
		Id uniqueidentifier NOT NULL,
		Parent uniqueidentifier NOT NULL,
		OrderIndexOld int NOT NULL,
		OrderIndexNew int NOT NULL
	)

	ALTER TABLE dbo.MainShiftActivityLayerFix306 
	ADD CONSTRAINT PK_MainShiftActivityLayerFix306 PRIMARY KEY (Batch, Id)
END

-------------------
--Save error layers
-------------------
DECLARE @Batch int
DECLARE @UpdatedOn smalldatetime
SELECT @UpdatedOn = GETDATE()
SELECT @Batch = ISNULL(MAX(Batch),0) FROM MainShiftActivityLayerFix306
SET @Batch = @Batch + 1

INSERT INTO dbo.MainShiftActivityLayerFix306
SELECT
		@Batch,
		@UpdatedOn,
		ps1.Id,
		ps1.Parent,
		ps1.OrderIndex as OrderIndexOld,
		MainShiftActivityLayerCheck.rn-1 as OrderindexNew
	FROM MainShiftActivityLayer ps1
	INNER JOIN
	(
		SELECT ps2.id, ps2.parent, ps2.orderindex, ROW_NUMBER()OVER(PARTITION BY ps2.parent ORDER BY ps2.orderindex) rn
		FROM MainShiftActivityLayer ps2
	) MainShiftActivityLayerCheck
	ON ps1.id=MainShiftActivityLayerCheck.id
	WHERE MainShiftActivityLayerCheck.OrderIndex <> MainShiftActivityLayerCheck.rn-1

-------------------
--Update error layers
-------------------
UPDATE MainShiftActivityLayer
SET 
	OrderIndex	= fix.OrderindexNew
FROM dbo.MainShiftActivityLayerFix306 fix
WHERE fix.Id = MainShiftActivityLayer.Id AND fix.Parent = MainShiftActivityLayer.Parent
AND fix.Batch = @Batch

-------------------
--Report error layers
-------------------
IF (SELECT COUNT(1) FROM dbo.MainShiftActivityLayerFix306 WHERE Batch=@Batch)> 0
PRINT 'Shifts have been updated'

----------------  
--Name: Anders F
--Date: 2010-12-03
--Desc: Bug #12670. Shifts and absence could be entered on a second level which cause ETL to fail.
--		Note: this script is delivered her (306 as a "function" with NO MERGE)
--			  It should and can be executed multiple times
--			  also added to root in the same manner
----------------  

update PersonAbsence
set Minimum = CAST(Minimum as smalldatetime),
	Maximum = CAST(Maximum as smalldatetime)
where (Minimum <> CAST(Minimum as smalldatetime)) or
           (Maximum <> CAST(Maximum as smalldatetime))

update MainShiftActivityLayer
set Minimum = CAST(Minimum as smalldatetime),
	Maximum = CAST(Maximum as smalldatetime)
where (Minimum <> CAST(Minimum as smalldatetime)) or
           (Maximum <> CAST(Maximum as smalldatetime))

update PersonalShiftActivityLayer
set Minimum = CAST(Minimum as smalldatetime),
	Maximum = CAST(Maximum as smalldatetime)
where (Minimum <> CAST(Minimum as smalldatetime)) or
           (Maximum <> CAST(Maximum as smalldatetime))

update OvertimeShiftActivityLayer
set Minimum = CAST(Minimum as smalldatetime),
	Maximum = CAST(Maximum as smalldatetime)
where (Minimum <> CAST(Minimum as smalldatetime)) or
           (Maximum <> CAST(Maximum as smalldatetime))

----------------  
--Name: Micke D
--Date: 2010-12-05
--Desc: Team Blue changes for max seat
----------------  

ALTER TABLE dbo.Activity ADD
RequiresSeat bit NOT NULL CONSTRAINT DF_Activity_RequiresSeat DEFAULT 0


ALTER TABLE dbo.Site ADD
MaxSeats int NULL
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (307,'7.1.307') 
