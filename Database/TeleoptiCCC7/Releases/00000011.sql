/*
BuildTime is:
2008-11-19
11:27
*/ 
/*
Trunk initiated:
2008-11-18
15:29
By: TOPTINET\davidj
On TELEOPTI625
*/
----------------
--Name: David J och Peter W
--Date: 2008-11-19
--Desc: Re-inititaded DB-versions
----------------

SET NOCOUNT ON
PRINT 'Adding tables. Working ...'

create table dbo.RtaStateGroup (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, DefaultStateGroup BIT not null, Available BIT not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.Setting (Id UNIQUEIDENTIFIER not null, Name NVARCHAR(50) not null, SerializedValue NVARCHAR(255) not null, TypeName NVARCHAR(255) not null, Person UNIQUEIDENTIFIER null, Parent UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.WorkShiftRuleSet (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, BaseActivity UNIQUEIDENTIFIER not null, Category UNIQUEIDENTIFIER not null, EarlyStart BIGINT not null, LateStart BIGINT not null, StartSegment BIGINT not null, EarlyEnd BIGINT not null, LateEnd BIGINT not null, EndSegment BIGINT not null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, DefaultAccessibility INT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.RuleSetRuleSetBag (RuleSet UNIQUEIDENTIFIER not null, RuleSetBag UNIQUEIDENTIFIER not null)
create table dbo.AccessibilityDaysOfWeek (RuleSet UNIQUEIDENTIFIER not null, DayOfWeek INT null)
create table dbo.AccessibilityDates (RuleSet UNIQUEIDENTIFIER not null, Date DATETIME null)
create table dbo.SkillDayTemplate (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, WeekdayIndex INT not null, Name NVARCHAR(50) not null, VersionNumber INT not null)--, primary key (Id))
create table dbo.WorkloadDayBase (Id UNIQUEIDENTIFIER not null, WorkloadDate DATETIME null, Workload UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.OpenHourList (Parent UNIQUEIDENTIFIER not null, Minimum BIGINT not null, Maximum BIGINT not null)
create table dbo.ApplicationFunction (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Parent UNIQUEIDENTIFIER null, FunctionCode NVARCHAR(50) not null, FunctionDescription NVARCHAR(255) null, ForeignId NVARCHAR(255) null, ForeignSource NVARCHAR(255) null, SortOrder INT null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.Contract (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, EmploymentType INT not null, AvgWorkTimePerDay BIGINT not null, MaxTimePerWeek BIGINT not null, NightlyRest BIGINT not null, WeeklyRest BIGINT not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.ExtraTimeDirectiveTime (Contract UNIQUEIDENTIFIER not null, MinTime BIGINT not null, ExtraTimeType INT not null, MaxTime BIGINT not null)--, primary key (Contract, ExtraTimeType))
create table dbo.ExtraTimeDirectiveAllowedTypes (Contract UNIQUEIDENTIFIER not null, ExtraTimeType INT null)
create table dbo.Absence (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, DisplayColor INT not null, Tracker TINYINT null, Priority TINYINT not null, InContractTime BIT not null, Requestable BIT not null, BusinessUnit UNIQUEIDENTIFIER not null, GroupingAbsence UNIQUEIDENTIFIER null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.OptionalColumnValue (Id UNIQUEIDENTIFIER not null, Description NVARCHAR(255) not null, ReferenceId UNIQUEIDENTIFIER null, Parent UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.KeyPerformanceIndicator (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ResourceKey NVARCHAR(50) not null, TargetValueType INT not null, DefaultTargetValue DOUBLE PRECISION not null, DefaultMinValue DOUBLE PRECISION not null, DefaultMaxValue DOUBLE PRECISION not null, DefaultBetweenColor INT not null, DefaultLowerThanMinColor INT not null, DefaultHigherThanMaxColor INT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.SkillDay (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, TemplateName NVARCHAR(50) null, TemplateId UNIQUEIDENTIFIER not null, DayOfWeek INT null, VersionNumber INT not null, TemplateReferenceSkill UNIQUEIDENTIFIER null, SkillDayDate DATETIME not null, Skill UNIQUEIDENTIFIER not null, Scenario UNIQUEIDENTIFIER not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))--,unique (SkillDayDate, Skill, Scenario))
create table dbo.TemplateTaskPeriod (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, Tasks DOUBLE PRECISION not null, AverageTaskTime BIGINT not null, AverageAfterTaskTime BIGINT not null, CampaignTasks DOUBLE PRECISION not null, CampaignTaskTime DOUBLE PRECISION not null, CampaignAfterTaskTime DOUBLE PRECISION not null, Minimum DATETIME not null, Maximum DATETIME not null)--, primary key (Id))
create table dbo.PersonSkill (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, Skill UNIQUEIDENTIFIER not null, Value DOUBLE PRECISION not null)--, primary key (Id))
create table dbo.StateGroupActivityAlarm (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Activity UNIQUEIDENTIFIER not null, StateGroup UNIQUEIDENTIFIER not null, AlarmType UNIQUEIDENTIFIER not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.PersonAvailability (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Person UNIQUEIDENTIFIER not null, StartDate DATETIME not null, StartDay INT not null, Rotation UNIQUEIDENTIFIER not null, IsDeleted BIT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.MainShiftActivityLayer (Id UNIQUEIDENTIFIER not null, payLoad UNIQUEIDENTIFIER not null, Minimum DATETIME not null, Maximum DATETIME not null, Parent UNIQUEIDENTIFIER not null, OrderIndex INT not null)--, primary key (Id))
create table dbo.CompensationLayer (Id UNIQUEIDENTIFIER not null, PayLoad UNIQUEIDENTIFIER not null, Minimum DATETIME not null, Maximum DATETIME not null, Parent UNIQUEIDENTIFIER not null, OrderIndex INT not null)--, primary key (Id))
create table dbo.AlarmType (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, DisplayColor INT not null, Mode INT not null, ThresholdTime BIGINT not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.DayOff (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, Flexibility BIGINT null, Anchor BIGINT null, TargetLength BIGINT null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.SkillType (Id UNIQUEIDENTIFIER not null, ForecastType INT not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, ForecastSource INT not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.ScheduleRestriction (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, PersonRestrictionMain UNIQUEIDENTIFIER null, RestrictionPartParent UNIQUEIDENTIFIER null, StartTimeMinimum BIGINT null, StartTimeMaximum BIGINT null, EndTimeMinimum BIGINT null, EndTimeMaximum BIGINT null, WorkTimeMinimum BIGINT not null, WorkTimeMaximum BIGINT not null)--, primary key (Id))
create table dbo.PreferenceRestriction (ScheduleRestriction UNIQUEIDENTIFIER not null, ShiftCategory UNIQUEIDENTIFIER null, Activity UNIQUEIDENTIFIER null, DayOff BIT not null)--, primary key (ScheduleRestriction))
create table dbo.ShiftCategory (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, DisplayColor INT not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.AvailabilityRestriction (ScheduleRestriction UNIQUEIDENTIFIER not null, Available BIT not null)--, primary key (ScheduleRestriction))
create table dbo.PersonRestriction (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Person UNIQUEIDENTIFIER not null, RestrictionDate DATETIME not null, IsDeleted BIT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.KpiTarget (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, KeyPerformanceIndicator UNIQUEIDENTIFIER not null, Team UNIQUEIDENTIFIER not null, TargetValue DOUBLE PRECISION not null, MinValue DOUBLE PRECISION not null, MaxValue DOUBLE PRECISION not null, BetweenColor INT not null, LowerThanMinColor INT not null, HigherThanMaxColor INT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.DayOffPlannerRules (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(255) not null, NumberOfDaysInPeriod INT not null, NumberOfDayOffsInPeriod INT not null, UseConsecutiveWorkdays BIT not null, ConsecutiveWorkdaysMinimum INT not null, ConsecutiveWorkdaysMaximum INT not null, UseDaysOffPerWeek BIT not null, DaysOffPerWeekMinimum INT not null, DaysOffPerWeekMaximum INT not null, UseConsecutiveDaysOff BIT not null, ConsecutiveDaysOffMinimum INT not null, ConsecutiveDaysOffMaximum INT not null, UsePreWeek BIT not null, UsePostWeek BIT not null, KeepWeekendsTogether BIT not null, UseFreeWeekends BIT not null, FreeWeekendsMinimum INT not null, FreeWeekendsMaximum INT not null, UseFreeWeekendDays BIT not null, FreeWeekendDaysMinimum INT not null, FreeWeekendDaysMaximum INT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.PersonAccount (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, StartDateTime DATETIME null, Name NVARCHAR(50) null, ShortName NVARCHAR(25) null)--, primary key (Id))
create table dbo.PersonAccountDay (PersonAccount UNIQUEIDENTIFIER not null, Accrued INT not null, Extra INT not null, BalanceIn INT not null)--, primary key (PersonAccount))
create table dbo.AvailabilityDayRestriction (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER null, StartTimeMinimum BIGINT null, StartTimeMaximum BIGINT null, EndTimeMinimum BIGINT null, EndTimeMaximum BIGINT null, WorkTimeMinimum BIGINT not null, WorkTimeMaximum BIGINT not null, Available BIT not null)--, primary key (Id))
create table dbo.RotationRestriction (ScheduleRestriction UNIQUEIDENTIFIER not null, ShiftCategory UNIQUEIDENTIFIER null, DayOff BIT not null)--, primary key (ScheduleRestriction))
create table dbo.TemplateMultisitePeriod (Id UNIQUEIDENTIFIER not null, Version INT not null, Parent UNIQUEIDENTIFIER not null, StartDateTime DATETIME not null, EndDateTime DATETIME not null)--, primary key (Id))
create table dbo.TemplateMultisitePeriodDistribution (Parent UNIQUEIDENTIFIER not null, Value DOUBLE PRECISION not null, ChildSkill UNIQUEIDENTIFIER not null)--, primary key (Parent, ChildSkill))
create table dbo.SystemRoleApplicationRoleMapper (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, SystemRoleLongName NVARCHAR(255) not null, SystemName NVARCHAR(255) null, ApplicationRole UNIQUEIDENTIFIER not null, IsDeleted BIT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.TemplateSkillDataPeriod (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, Value DOUBLE PRECISION not null, Seconds DOUBLE PRECISION not null, MaxOccupancy DOUBLE PRECISION not null, MinOccupancy DOUBLE PRECISION not null, PersonBoundary_Maximum INT not null, PersonBoundary_Minimum INT not null, Shrinkage DOUBLE PRECISION not null, StartDateTime DATETIME not null, EndDateTime DATETIME not null)--, primary key (Id))
create table dbo.PartTimePercentage (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, Value DOUBLE PRECISION null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.PersonCompensation (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Person UNIQUEIDENTIFIER not null, Scenario UNIQUEIDENTIFIER not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.Compensation (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, DisplayColor INT not null, Factor DOUBLE PRECISION not null, CompensationType INT not null, ExtraTimeType INT not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.PersonDayOff (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Anchor DATETIME null, TargetLength BIGINT null, Value DOUBLE PRECISION null, Person UNIQUEIDENTIFIER not null, Scenario UNIQUEIDENTIFIER not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.PersonAssignment (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Person UNIQUEIDENTIFIER not null, Scenario UNIQUEIDENTIFIER not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.ActivityTimeLimiter (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, TimeLimit BIGINT not null, Activity UNIQUEIDENTIFIER not null, TimeLimitOperator INT not null)--, primary key (Id))
create table dbo.PersonGroupBase (Id UNIQUEIDENTIFIER not null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null)--, primary key (Id))
create table dbo.PersonGroup (PersonGroup UNIQUEIDENTIFIER not null, Person UNIQUEIDENTIFIER not null)
create table dbo.RootPersonGroup (PersonGroupBase UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null)--, primary key (PersonGroupBase))
create table dbo.ChildPersonGroup (PersonGroupBase UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null)--, primary key (PersonGroupBase))
create table dbo.ContractSchedule (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.TimeActivityLayer (Id UNIQUEIDENTIFIER not null, payLoad UNIQUEIDENTIFIER not null, Minimum DATETIME not null, Maximum DATETIME not null, Parent UNIQUEIDENTIFIER not null, OrderIndex INT not null)--, primary key (Id))
create table dbo.RtaState (Id UNIQUEIDENTIFIER not null, Name NVARCHAR(50) not null, StateCode NVARCHAR(50) not null, PlatformTypeId UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.PersonRotation (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Person UNIQUEIDENTIFIER not null, StartDate DATETIME not null, StartDay INT not null, Rotation UNIQUEIDENTIFIER not null, IsDeleted BIT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.Outlier (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, Workload UNIQUEIDENTIFIER null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.OutlierDates (Parent UNIQUEIDENTIFIER not null, Date DATETIME null)
create table dbo.MultisiteDayTemplate (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER null, Name NVARCHAR(50) not null, WeekdayIndex INT not null, VersionNumber INT not null)--, primary key (Id))
create table dbo.AvailableData (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, ApplicationRole UNIQUEIDENTIFIER not null, AvailableDataRange INT not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.AvailablePersonsInApplicationRole (AvailableData UNIQUEIDENTIFIER not null, AvailablePerson UNIQUEIDENTIFIER null, collection_id UNIQUEIDENTIFIER not null)--, primary key (collection_id))
create table dbo.AvailableTeamsInApplicationRole (AvailableData UNIQUEIDENTIFIER not null, AvailableTeam UNIQUEIDENTIFIER null, collection_id UNIQUEIDENTIFIER not null)--, primary key (collection_id))
create table dbo.AvailableSitesInApplicationRole (AvailableData UNIQUEIDENTIFIER not null, AvailableSite UNIQUEIDENTIFIER null, collection_id UNIQUEIDENTIFIER not null)--, primary key (collection_id))
create table dbo.AvailableUnitsInApplicationRole (AvailableData UNIQUEIDENTIFIER not null, AvailableBusinessUnit UNIQUEIDENTIFIER null, collection_id UNIQUEIDENTIFIER not null)--, primary key (collection_id))
create table dbo.PersonAbsence (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, LastChange DATETIME null, Person UNIQUEIDENTIFIER not null, Scenario UNIQUEIDENTIFIER not null, PayLoad UNIQUEIDENTIFIER not null, Minimum DATETIME not null, Maximum DATETIME not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.Team (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Site UNIQUEIDENTIFIER not null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, SchedulePublishedToDate DATETIME null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.MainShift (Id UNIQUEIDENTIFIER not null, Name NVARCHAR(50) not null, ShiftCategory UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.CommonNameDescriptionSetting (Setting UNIQUEIDENTIFIER not null)--, primary key (Setting))
create table dbo.Meeting (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Organizer UNIQUEIDENTIFIER not null, Scenario UNIQUEIDENTIFIER not null, Subject NVARCHAR(100) not null, Description NVARCHAR(2000) not null, Location NVARCHAR(100) not null, PayLoad UNIQUEIDENTIFIER not null, Minimum DATETIME not null, Maximum DATETIME not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.ActivityExtender (Id UNIQUEIDENTIFIER not null, ExtenderType NVARCHAR(255) not null, Parent UNIQUEIDENTIFIER not null, ExtendWithActivity UNIQUEIDENTIFIER not null, MinimumLength BIGINT not null, MaximumLength BIGINT not null, LengthSegment BIGINT not null, OrderIndex INT not null, NumberOfLayers TINYINT null, AutoPosStartSegment BIGINT null, EarlyStart BIGINT null, LateStart BIGINT null, StartSegment BIGINT null)--, primary key (Id))
create table dbo.ValidatedVolumeDay (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, VolumeDayDate DATETIME not null, Workload UNIQUEIDENTIFIER not null, ValidatedTasks DOUBLE PRECISION null, ValidatedTaskTime BIGINT null, ValidatedAfterTaskTime BIGINT null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.SchedulePeriod (Id UNIQUEIDENTIFIER not null, DateFrom DATETIME not null, PeriodType INT not null, Number INT not null, AverageWorkTimePerDay BIGINT null, DaysOff INT null, Parent UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.PersonPeriod (Id UNIQUEIDENTIFIER not null, StartDate DATETIME not null, Team UNIQUEIDENTIFIER not null, Note NVARCHAR(1024) null, Parent UNIQUEIDENTIFIER not null, RuleSetBag UNIQUEIDENTIFIER null, PartTimePercentage UNIQUEIDENTIFIER not null, Contract UNIQUEIDENTIFIER not null, ContractSchedule UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.ExternalLogOnCollection (PersonPeriod UNIQUEIDENTIFIER not null, ExternalLogOn UNIQUEIDENTIFIER not null)--,unique (PersonPeriod, ExternalLogOn))
create table dbo.WorkloadDay (WorkloadDayBase UNIQUEIDENTIFIER not null, Scenario UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, TemplateName NVARCHAR(50) null, TemplateId UNIQUEIDENTIFIER not null, DayOfWeek INT null, VersionNumber INT not null, Workload UNIQUEIDENTIFIER null, Annotation NVARCHAR(1000) null)--, primary key (WorkloadDayBase))
create table dbo.Site (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, BusinessUnit UNIQUEIDENTIFIER not null, TimeZone NVARCHAR(50) not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.Skill (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, DisplayColor INT not null, Description NVARCHAR(1024) not null, DefaultResolution INT not null, SkillType UNIQUEIDENTIFIER not null, Activity UNIQUEIDENTIFIER not null, TimeZone NVARCHAR(50) not null, MidnightBreakOffset BIGINT not null, SeriousUnderstaffing DOUBLE PRECISION not null, Understaffing DOUBLE PRECISION not null, Overstaffing DOUBLE PRECISION not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.ChildSkill (Skill UNIQUEIDENTIFIER not null, ParentSkill UNIQUEIDENTIFIER null)--, primary key (Skill))
create table dbo.GroupPage (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.Availability (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.PersonTimeActivity (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Person UNIQUEIDENTIFIER not null, Scenario UNIQUEIDENTIFIER not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.TimeActivity (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, DisplayColor INT not null, InfluenceType INT not null, ExtraTimeType INT not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.GroupingAbsence (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.MeetingPerson (Id UNIQUEIDENTIFIER not null, Person UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, Optional BIT not null)--, primary key (Id))
create table dbo.ExternalLogOn (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, LogOnId INT not null, LogOnAggId INT not null, LogOnCode NVARCHAR(50) null, LogOnName NVARCHAR(100) null, Active BIT not null, DataSourceId INT not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.MultisiteSkill (Skill UNIQUEIDENTIFIER not null)--, primary key (Skill))
create table dbo.Scorecard (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(255) not null, Period INT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.KeyPerformanceIndicatorCollection (Scorecard UNIQUEIDENTIFIER not null, KeyPerformanceIndicator UNIQUEIDENTIFIER not null)--,unique (Scorecard, KeyPerformanceIndicator))
create table dbo.GroupingActivity (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.Workload (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, Description NVARCHAR(1024) not null, Skill UNIQUEIDENTIFIER not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.QueueSourceCollection (Workload UNIQUEIDENTIFIER not null, QueueSource UNIQUEIDENTIFIER not null)--,unique (Workload, QueueSource))
create table dbo.PersonalShift (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, OrderIndex INT not null)--, primary key (Id))
create table dbo.RotationDay (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, Day INT null)--, primary key (Id))
create table dbo.RequestPart (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.AbsenceRequest (RequestPart UNIQUEIDENTIFIER not null, Absence UNIQUEIDENTIFIER not null, StartDateTime DATETIME not null, EndDateTime DATETIME not null)--, primary key (RequestPart))
create table dbo.ShiftTradeRequest (RequestPart UNIQUEIDENTIFIER not null, StartDateTime DATETIME not null, EndDateTime DATETIME not null, RequestedPerson UNIQUEIDENTIFIER not null, RequestStatus INT not null)--, primary key (RequestPart))
create table dbo.TextRequest (RequestPart UNIQUEIDENTIFIER not null, StartDateTime DATETIME not null, EndDateTime DATETIME not null)--, primary key (RequestPart))
create table dbo.OutlierDateProviderBase (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER null)--, primary key (Id))
create table dbo.MultisitePeriod (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, StartDateTime DATETIME not null, EndDateTime DATETIME not null)--, primary key (Id))
create table dbo.MultisitePeriodDistribution (Parent UNIQUEIDENTIFIER not null, Value DOUBLE PRECISION not null, ChildSkill UNIQUEIDENTIFIER not null)--, primary key (Parent, ChildSkill))
create table dbo.RuleSetBag (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.SkillDataPeriod (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, Value DOUBLE PRECISION not null, Seconds DOUBLE PRECISION not null, MaxOccupancy DOUBLE PRECISION not null, MinOccupancy DOUBLE PRECISION not null, PersonBoundary_Maximum INT not null, PersonBoundary_Minimum INT not null, Shrinkage DOUBLE PRECISION not null, StartDateTime DATETIME not null, EndDateTime DATETIME not null)--, primary key (Id))
create table dbo.Activity (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, DisplayColor INT not null, InContractTime BIT not null, InReadyTime BIT not null, RequiresSkill BIT not null, BusinessUnit UNIQUEIDENTIFIER not null, GroupingActivity UNIQUEIDENTIFIER null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.PersonAccountTime (PersonAccount UNIQUEIDENTIFIER not null, Accrued BIGINT not null, Extra BIGINT not null, BalanceIn BIGINT not null)--, primary key (PersonAccount))
create table dbo.CommonSettings (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Category NVARCHAR(50) not null, Name NVARCHAR(50) not null, Value NVARCHAR(255) not null, DefaultValue NVARCHAR(255) null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.QueueSource (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, CtiQueueId INT not null, DataSourceId INT null, QueueAggId INT null, DataSource NVARCHAR(50) null, Name NVARCHAR(50) not null, Description NVARCHAR(50) not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.ChartSetting (Setting UNIQUEIDENTIFIER not null)--, primary key (Setting))
create table dbo.SettingCategory (Id UNIQUEIDENTIFIER not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, Parent UNIQUEIDENTIFIER null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.AvailabilityDay (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, Day INT null)--, primary key (Id))
create table dbo.Rotation (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.RotationDayRestriction (Id UNIQUEIDENTIFIER not null, StartTimeMinimum BIGINT null, StartTimeMaximum BIGINT null, EndTimeMinimum BIGINT null, EndTimeMaximum BIGINT null, WorkTimeMinimum BIGINT not null, WorkTimeMaximum BIGINT not null, Parent UNIQUEIDENTIFIER null, ShiftCategory UNIQUEIDENTIFIER null, DayOff BIT not null)--, primary key (Id))
create table dbo.PersonRequest (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Person UNIQUEIDENTIFIER not null, RequestStatus INT not null, Subject NVARCHAR(80) null, Message NVARCHAR(300) null, IsDeleted BIT not null, BusinessUnit UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.PersonalShiftActivityLayer (Id UNIQUEIDENTIFIER not null, payLoad UNIQUEIDENTIFIER not null, Minimum DATETIME not null, Maximum DATETIME not null, Parent UNIQUEIDENTIFIER not null, OrderIndex INT not null)--, primary key (Id))
create table dbo.Person (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Email NVARCHAR(50) not null, Note NVARCHAR(1024) not null, EmploymentNumber NVARCHAR(50) not null, TerminalDate DATETIME null, FirstName NVARCHAR(25) not null, LastName NVARCHAR(25) not null, PartOfUnique UNIQUEIDENTIFIER null, DefaultTimeZone NVARCHAR(50) null, Culture INT null, UiCulture INT null, WindowsLogOnName NVARCHAR(50) null, DomainName NVARCHAR(50) null, ApplicationLogOnName NVARCHAR(50) null, Password NVARCHAR(50) null, IsDeleted BIT not null)--, primary key (Id))--,unique (PartOfUnique, WindowsLogOnName, DomainName, ApplicationLogOnName))
create table dbo.PersonInApplicationRole (Person UNIQUEIDENTIFIER not null, ApplicationRole UNIQUEIDENTIFIER not null)--,unique (Person, ApplicationRole))
create table dbo.Scenario (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, DefaultScenario BIT not null, Audittrail BIT not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.ChartSeriesSetting (Id UNIQUEIDENTIFIER not null, DisplayKey NVARCHAR(50) not null, Enabled BIT not null, SeriesType INT not null, AxisLocation INT not null, Color INT not null, Parent UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.ContractTimeLimiter (Id UNIQUEIDENTIFIER not null, Parent UNIQUEIDENTIFIER not null, MinimumLength BIGINT not null, MaximumLength BIGINT not null, LengthSegment BIGINT not null)--, primary key (Id))
create table dbo.OptionalColumn (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(255) not null, TableName NVARCHAR(255) not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.MultisiteDay (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, TemplateName NVARCHAR(50) null, TemplateId UNIQUEIDENTIFIER not null, DayOfWeek INT null, VersionNumber INT not null, TemplateReferenceSkill UNIQUEIDENTIFIER null, MultisiteDayDate DATETIME not null, Skill UNIQUEIDENTIFIER not null, Scenario UNIQUEIDENTIFIER not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))--,unique (MultisiteDayDate, Skill, Scenario))
create table dbo.WorkloadDayTemplate (WorkloadDayBase UNIQUEIDENTIFIER not null, Name NVARCHAR(50) not null, CreatedDate DATETIME null, Parent UNIQUEIDENTIFIER null, WeekdayIndex INT not null, VersionNumber INT not null)--, primary key (WorkloadDayBase))
create table dbo.ApplicationRole (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, DescriptionText NVARCHAR(255) null, BuiltIn BIT not null, BusinessUnit UNIQUEIDENTIFIER not null, IsDeleted BIT not null)--, primary key (Id))
create table dbo.ApplicationFunctionInRole (ApplicationRole UNIQUEIDENTIFIER not null, ApplicationFunction UNIQUEIDENTIFIER not null)--,unique (ApplicationRole, ApplicationFunction))
create table dbo.ContractScheduleWeek (Id UNIQUEIDENTIFIER not null, WeekOrder INT not null, Parent UNIQUEIDENTIFIER not null)--, primary key (Id))
create table dbo.ContractScheduleWeekWorkDays (ContractScheduleWeek UNIQUEIDENTIFIER not null, WorkDay BIT null, DayOfWeek INT not null)--, primary key (ContractScheduleWeek, DayOfWeek))
create table dbo.BusinessUnit (Id UNIQUEIDENTIFIER not null, Version INT not null, CreatedBy UNIQUEIDENTIFIER not null, UpdatedBy UNIQUEIDENTIFIER null, CreatedOn DATETIME not null, UpdatedOn DATETIME null, Name NVARCHAR(50) not null, ShortName NVARCHAR(25) null, IsDeleted BIT not null)--, primary key (Id))

--PK
ALTER TABLE [dbo].[Absence] ADD CONSTRAINT PK_Absence PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[AbsenceRequest] ADD CONSTRAINT PK_AbsenceRequest PRIMARY KEY CLUSTERED 
(
	[RequestPart] ASC
)
ALTER TABLE [dbo].[Activity] ADD CONSTRAINT PK_Activity PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ActivityExtender] ADD CONSTRAINT PK_ActivityExtender PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ActivityTimeLimiter] ADD CONSTRAINT PK_ActivityTimeLimiter PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[AlarmType] ADD CONSTRAINT PK_AlarmType PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ApplicationFunction] ADD CONSTRAINT PK_ApplicationFunction PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ApplicationRole] ADD CONSTRAINT PK_ApplicationRole PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Availability] ADD CONSTRAINT PK_Availability PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[AvailabilityDay] ADD CONSTRAINT PK_AvailabilityDay PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[AvailabilityDayRestriction] ADD CONSTRAINT PK_AvailabilityDayRestriction PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[AvailabilityRestriction] ADD CONSTRAINT PK_AvailabilityRestriction PRIMARY KEY CLUSTERED 
(
	[ScheduleRestriction] ASC
)
ALTER TABLE [dbo].[AvailableData] ADD CONSTRAINT PK_AvailableData PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[AvailablePersonsInApplicationRole] ADD CONSTRAINT PK_AvailablePersonsInApplicationRole PRIMARY KEY CLUSTERED 
(
	[collection_id] ASC
)
ALTER TABLE [dbo].[AvailableSitesInApplicationRole] ADD CONSTRAINT PK_AvailableSitesInApplicationRole PRIMARY KEY CLUSTERED 
(
	[collection_id] ASC
)
ALTER TABLE [dbo].[AvailableTeamsInApplicationRole] ADD CONSTRAINT PK_AvailableTeamsInApplicationRole PRIMARY KEY CLUSTERED 
(
	[collection_id] ASC
)
ALTER TABLE [dbo].[AvailableUnitsInApplicationRole] ADD CONSTRAINT PK_AvailableUnitsInApplicationRole PRIMARY KEY CLUSTERED 
(
	[collection_id] ASC
)
ALTER TABLE [dbo].[BusinessUnit] ADD CONSTRAINT PK_BusinessUnit PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ChartSeriesSetting] ADD CONSTRAINT PK_ChartSeriesSetting PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ChartSetting] ADD CONSTRAINT PK_ChartSetting PRIMARY KEY CLUSTERED 
(
	[Setting] ASC
)
ALTER TABLE [dbo].[ChildPersonGroup] ADD CONSTRAINT PK_ChildPersonGroup PRIMARY KEY CLUSTERED 
(
	[PersonGroupBase] ASC
)
ALTER TABLE [dbo].[ChildSkill] ADD CONSTRAINT PK_ChildSkill PRIMARY KEY CLUSTERED 
(
	[Skill] ASC
)
ALTER TABLE [dbo].[CommonNameDescriptionSetting] ADD CONSTRAINT PK_CommonNameDescriptionSetting PRIMARY KEY CLUSTERED 
(
	[Setting] ASC
)
ALTER TABLE [dbo].[CommonSettings] ADD CONSTRAINT PK_CommonSettings PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Compensation] ADD CONSTRAINT PK_Compensation PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[CompensationLayer] ADD CONSTRAINT PK_CompensationLayer PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Contract] ADD CONSTRAINT PK_Contract PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ContractSchedule] ADD CONSTRAINT PK_ContractSchedule PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ContractScheduleWeek] ADD CONSTRAINT PK_ContractScheduleWeek PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ContractTimeLimiter] ADD CONSTRAINT PK_ContractTimeLimiter PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[DayOff] ADD CONSTRAINT PK_DayOff PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[DayOffPlannerRules] ADD CONSTRAINT PK_DayOffPlannerRules PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ExternalLogOn] ADD CONSTRAINT PK_ExternalLogOn PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[GroupingAbsence] ADD CONSTRAINT PK_GroupingAbsence PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[GroupingActivity] ADD CONSTRAINT PK_GroupingActivity PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[GroupPage] ADD CONSTRAINT PK_GroupPage PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[KeyPerformanceIndicator] ADD CONSTRAINT PK_KeyPerformanceIndicator PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[KpiTarget] ADD CONSTRAINT PK_KpiTarget PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[MainShift] ADD CONSTRAINT PK_MainShift PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[MainShiftActivityLayer] ADD CONSTRAINT PK_MainShiftActivityLayer PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Meeting] ADD CONSTRAINT PK_Meeting PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[MeetingPerson] ADD CONSTRAINT PK_MeetingPerson PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[MultisiteDay] ADD CONSTRAINT PK_MultisiteDay PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[MultisiteDayTemplate] ADD CONSTRAINT PK_MultisiteDayTemplate PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[MultisitePeriod] ADD CONSTRAINT PK_MultisitePeriod PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[MultisiteSkill] ADD CONSTRAINT PK_MultisiteSkill PRIMARY KEY CLUSTERED 
(
	[Skill] ASC
)
ALTER TABLE [dbo].[OptionalColumn] ADD CONSTRAINT PK_OptionalColumn PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[OptionalColumnValue] ADD CONSTRAINT PK_OptionalColumnValue PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Outlier] ADD CONSTRAINT PK_Outlier PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[OutlierDateProviderBase] ADD CONSTRAINT PK_OutlierDateProviderBase PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PartTimePercentage] ADD CONSTRAINT PK_PartTimePercentage PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Person] ADD CONSTRAINT PK_Person PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonAbsence] ADD CONSTRAINT PK_PersonAbsence PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonAccount] ADD CONSTRAINT PK_PersonAccount PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonAccountDay] ADD CONSTRAINT PK_PersonAccountDay PRIMARY KEY CLUSTERED 
(
	[PersonAccount] ASC
)
ALTER TABLE [dbo].[PersonAccountTime] ADD CONSTRAINT PK_PersonAccountTime PRIMARY KEY CLUSTERED 
(
	[PersonAccount] ASC
)
ALTER TABLE [dbo].[PersonalShift] ADD CONSTRAINT PK_PersonalShift PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonalShiftActivityLayer] ADD CONSTRAINT PK_PersonalShiftActivityLayer PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonAssignment] ADD CONSTRAINT PK_PersonAssignment PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonAvailability] ADD CONSTRAINT PK_PersonAvailability PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonCompensation] ADD CONSTRAINT PK_PersonCompensation PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonDayOff] ADD CONSTRAINT PK_PersonDayOff PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonGroupBase] ADD CONSTRAINT PK_PersonGroupBase PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonPeriod] ADD CONSTRAINT PK_PersonPeriod PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonRequest] ADD CONSTRAINT PK_PersonRequest PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonRestriction] ADD CONSTRAINT PK_PersonRestriction PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonRotation] ADD CONSTRAINT PK_PersonRotation PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonSkill] ADD CONSTRAINT PK_PersonSkill PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PersonTimeActivity] ADD CONSTRAINT PK_PersonTimeActivity PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[PreferenceRestriction] ADD CONSTRAINT PK_PreferenceRestriction PRIMARY KEY CLUSTERED 
(
	[ScheduleRestriction] ASC
)
ALTER TABLE [dbo].[QueueSource] ADD CONSTRAINT PK_QueueSource PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[RequestPart] ADD CONSTRAINT PK_RequestPart PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[RootPersonGroup] ADD CONSTRAINT PK_RootPersonGroup PRIMARY KEY CLUSTERED 
(
	[PersonGroupBase] ASC
)
ALTER TABLE [dbo].[Rotation] ADD CONSTRAINT PK_Rotation PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[RotationDay] ADD CONSTRAINT PK_RotationDay PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[RotationDayRestriction] ADD CONSTRAINT PK_RotationDayRestriction PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[RotationRestriction] ADD CONSTRAINT PK_RotationRestriction PRIMARY KEY CLUSTERED 
(
	[ScheduleRestriction] ASC
)
ALTER TABLE [dbo].[RtaState] ADD CONSTRAINT PK_RtaState PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[RtaStateGroup] ADD CONSTRAINT PK_RtaStateGroup PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[RuleSetBag] ADD CONSTRAINT PK_RuleSetBag PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Scenario] ADD CONSTRAINT PK_Scenario PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[SchedulePeriod] ADD CONSTRAINT PK_SchedulePeriod PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ScheduleRestriction] ADD CONSTRAINT PK_ScheduleRestriction PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Scorecard] ADD CONSTRAINT PK_Scorecard PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Setting] ADD CONSTRAINT PK_Setting PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[SettingCategory] ADD CONSTRAINT PK_SettingCategory PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ShiftCategory] ADD CONSTRAINT PK_ShiftCategory PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ShiftTradeRequest] ADD CONSTRAINT PK_ShiftTradeRequest PRIMARY KEY CLUSTERED 
(
	[RequestPart] ASC
)
ALTER TABLE [dbo].[Site] ADD CONSTRAINT PK_Site PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Skill] ADD CONSTRAINT PK_Skill PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[SkillDataPeriod] ADD CONSTRAINT PK_SkillDataPeriod PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[SkillDay] ADD CONSTRAINT PK_SkillDay PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[SkillDayTemplate] ADD CONSTRAINT PK_SkillDayTemplate PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[SkillType] ADD CONSTRAINT PK_SkillType PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[StateGroupActivityAlarm] ADD CONSTRAINT PK_StateGroupActivityAlarm PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[SystemRoleApplicationRoleMapper] ADD CONSTRAINT PK_SystemRoleApplicationRoleMapper PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Team] ADD CONSTRAINT PK_Team PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[TemplateMultisitePeriod] ADD CONSTRAINT PK_TemplateMultisitePeriod PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[TemplateSkillDataPeriod] ADD CONSTRAINT PK_TemplateSkillDataPeriod PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[TemplateTaskPeriod] ADD CONSTRAINT PK_TemplateTaskPeriod PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[TextRequest] ADD CONSTRAINT PK_TextRequest PRIMARY KEY CLUSTERED 
(
	[RequestPart] ASC
)
ALTER TABLE [dbo].[TimeActivity] ADD CONSTRAINT PK_TimeActivity PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[TimeActivityLayer] ADD CONSTRAINT PK_TimeActivityLayer PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[ValidatedVolumeDay] ADD CONSTRAINT PK_ValidatedVolumeDay PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[Workload] ADD CONSTRAINT PK_Workload PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[WorkloadDay] ADD CONSTRAINT PK_WorkloadDay PRIMARY KEY CLUSTERED 
(
	[WorkloadDayBase] ASC
)
ALTER TABLE [dbo].[WorkloadDayBase] ADD CONSTRAINT PK_WorkloadDayBase PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ALTER TABLE [dbo].[WorkloadDayTemplate] ADD CONSTRAINT PK_WorkloadDayTemplate PRIMARY KEY CLUSTERED 
(
	[WorkloadDayBase] ASC
)
ALTER TABLE [dbo].[WorkShiftRuleSet] ADD CONSTRAINT PK_WorkShiftRuleSet PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)

--PK2, Composite Keys
ALTER TABLE [dbo].[ContractScheduleWeekWorkDays] ADD CONSTRAINT PK_ContractScheduleWeekWorkDays PRIMARY KEY CLUSTERED 
(
	[ContractScheduleWeek] ASC,
	[DayOfWeek] ASC
)

ALTER TABLE [dbo].[ExtraTimeDirectiveTime] ADD CONSTRAINT PK_ExtraTimeDirectiveTime PRIMARY KEY CLUSTERED 
(
	[Contract] ASC,
	[ExtraTimeType] ASC
)

ALTER TABLE [dbo].[MultisitePeriodDistribution] ADD CONSTRAINT PK_MultisitePeriodDistribution PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[ChildSkill] ASC
)

ALTER TABLE [dbo].[TemplateMultisitePeriodDistribution] ADD CONSTRAINT PK_TemplateMultisitePeriodDistribution PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[ChildSkill] ASC
)


--UQ
ALTER TABLE [dbo].[ApplicationFunctionInRole] ADD CONSTRAINT UQ_ApplicationFunctionInRole_ApplicationRole UNIQUE NONCLUSTERED 
(
	[ApplicationRole] ASC,
	[ApplicationFunction] ASC
)


ALTER TABLE [dbo].[ExternalLogOnCollection] ADD CONSTRAINT UQ_ExternalLogOnCollection_PersonPeriod UNIQUE NONCLUSTERED 
(
	[PersonPeriod] ASC,
	[ExternalLogOn] ASC
)

ALTER TABLE [dbo].[KeyPerformanceIndicatorCollection] ADD CONSTRAINT UQ_KeyPerformanceIndicatorCollection_Scorecard UNIQUE NONCLUSTERED 
(
	[Scorecard] ASC,
	[KeyPerformanceIndicator] ASC
)

ALTER TABLE [dbo].[MultisiteDay] ADD CONSTRAINT UQ_MultisiteDay_MultisiteDayDate UNIQUE NONCLUSTERED 
(
	[MultisiteDayDate] ASC,
	[Skill] ASC,
	[Scenario] ASC
)

ALTER TABLE [dbo].[Person] ADD CONSTRAINT UQ_Person_PartOfUnique UNIQUE NONCLUSTERED 
(
	[PartOfUnique] ASC,
	[WindowsLogOnName] ASC,
	[DomainName] ASC,
	[ApplicationLogOnName] ASC
)

ALTER TABLE [dbo].[PersonInApplicationRole] ADD CONSTRAINT UQ_PersonInApplicationRole_Person UNIQUE NONCLUSTERED 
(
	[Person] ASC,
	[ApplicationRole] ASC
)


ALTER TABLE [dbo].[QueueSourceCollection] ADD CONSTRAINT UQ_QueueSourceCollection_Workload UNIQUE NONCLUSTERED 
(
	[Workload] ASC,
	[QueueSource] ASC
)

ALTER TABLE [dbo].[SkillDay] ADD CONSTRAINT UQ_SkillDay_SkillDayDate UNIQUE NONCLUSTERED 
(
	[SkillDayDate] ASC,
	[Skill] ASC,
	[Scenario] ASC
)


--FK
alter table dbo.RtaStateGroup add constraint FK_RtaStateGroup_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.RtaStateGroup add constraint FK_RtaStateGroup_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.RtaStateGroup add constraint FK_RtaStateGroup_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.Setting add constraint FK_Setting_Person foreign key (Person) references Person
alter table dbo.Setting add constraint FK_Setting_SettingCategory foreign key (Parent) references SettingCategory
alter table dbo.WorkShiftRuleSet add constraint FK_WorkShiftRuleSet_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.WorkShiftRuleSet add constraint FK_WorkShiftRuleSet_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.WorkShiftRuleSet add constraint FK_WorkShiftRuleSet_Activity foreign key (BaseActivity) references Activity
alter table dbo.WorkShiftRuleSet add constraint FK_WorkShiftRuleSet_ShiftCategory foreign key (Category) references ShiftCategory
alter table dbo.WorkShiftRuleSet add constraint FK_WorkShiftRuleSet_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.RuleSetRuleSetBag add constraint FK_RuleSetRuleSetBag_RuleSetBag foreign key (RuleSetBag) references RuleSetBag
alter table dbo.RuleSetRuleSetBag add constraint FK_RuleSetRuleSetBag_WorkShiftRuleSet foreign key (RuleSet) references WorkShiftRuleSet
alter table dbo.AccessibilityDaysOfWeek add constraint FK_AccessibilityDaysOfWeek_WorkShiftRuleSet foreign key (RuleSet) references WorkShiftRuleSet
alter table dbo.AccessibilityDates add constraint FK_AccessibilityDates_WorkShiftRuleSet foreign key (RuleSet) references WorkShiftRuleSet
alter table dbo.SkillDayTemplate add constraint FK_SkillDayTemplate_Skill foreign key (Parent) references Skill
alter table dbo.WorkloadDayBase add constraint FK_WorkloadDayBase_Workload foreign key (Workload) references Workload
alter table dbo.OpenHourList add constraint FK_OpenHourList_WorkloadDayBase foreign key (Parent) references WorkloadDayBase
alter table dbo.ApplicationFunction add constraint FK_ApplicationFunction_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.ApplicationFunction add constraint FK_ApplicationFunction_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.ApplicationFunction add constraint FK_ApplicationFunction_ApplicationFunction foreign key (Parent) references ApplicationFunction
alter table dbo.Contract add constraint FK_Contract_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Contract add constraint FK_Contract_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Contract add constraint FK_Contract_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.ExtraTimeDirectiveTime add constraint FK_ExtraTimeDirectiveTime_Contract_MaxTime foreign key (Contract) references Contract
alter table dbo.ExtraTimeDirectiveAllowedTypes add constraint FK_ExtraTimeDirectiveAllowedTypes_Contract foreign key (Contract) references Contract
alter table dbo.Absence add constraint FK_Absence_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Absence add constraint FK_Absence_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Absence add constraint FK_Absence_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.Absence add constraint FK_Absence_GroupingAbsence foreign key (GroupingAbsence) references GroupingAbsence
alter table dbo.OptionalColumnValue add constraint FK_OptionalColumnValue_OptionalColumn foreign key (Parent) references OptionalColumn
alter table dbo.KeyPerformanceIndicator add constraint FK_KeyPerformanceIndicator_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.KeyPerformanceIndicator add constraint FK_KeyPerformanceIndicator_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.KeyPerformanceIndicator add constraint FK_KeyPerformanceIndicator_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.SkillDay add constraint FK_SkillDay_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.SkillDay add constraint FK_SkillDay_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.SkillDay add constraint FK_SkillDayTemplateReference_Skill foreign key (TemplateReferenceSkill) references Skill
alter table dbo.SkillDay add constraint FK_SkillDay_Skill foreign key (Skill) references Skill
alter table dbo.SkillDay add constraint FK_SkillDay_Scenario foreign key (Scenario) references Scenario
alter table dbo.SkillDay add constraint FK_SkillDay_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.TemplateTaskPeriod add constraint FK_TemplateTaskPeriod_WorkloadDayBase foreign key (Parent) references WorkloadDayBase
alter table dbo.PersonSkill add constraint FK_PersonSkill_PersonPeriod foreign key (Parent) references PersonPeriod
alter table dbo.PersonSkill add constraint FK_PersonSkill_Skill foreign key (Skill) references Skill
alter table dbo.StateGroupActivityAlarm add constraint FK_StateGroupActivityAlarm_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.StateGroupActivityAlarm add constraint FK_StateGroupActivityAlarm_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.StateGroupActivityAlarm add constraint FK_StateGroupActivityAlarm_Activity foreign key (Activity) references Activity
alter table dbo.StateGroupActivityAlarm add constraint FK_StateGroupActivityAlarm_RtaStateGroup foreign key (StateGroup) references RtaStateGroup
alter table dbo.StateGroupActivityAlarm add constraint FK_StateGroupActivityAlarm_AlarmType foreign key (AlarmType) references AlarmType
alter table dbo.StateGroupActivityAlarm add constraint FK_StateGroupActivityAlarm_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.PersonAvailability add constraint FK_PersonAvailability_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PersonAvailability add constraint FK_PersonAvailability_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonAvailability add constraint FK_PersonAvailability_Person3 foreign key (Person) references Person
alter table dbo.PersonAvailability add constraint FK_PersonAvailability_Availability foreign key (Rotation) references Availability
alter table dbo.PersonAvailability add constraint FK_PersonAvailability_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.MainShiftActivityLayer add constraint FK_MainShiftActivityLayer_Activity foreign key (payLoad) references Activity
alter table dbo.MainShiftActivityLayer add constraint FK_MainShiftActivityLayer_MainShift foreign key (Parent) references MainShift
alter table dbo.CompensationLayer add constraint FK_CompensationLayer_Compensation foreign key (PayLoad) references Compensation
alter table dbo.CompensationLayer add constraint FK_CompensationLayer_PersonCompensation foreign key (Parent) references PersonCompensation
alter table dbo.AlarmType add constraint FK_AlarmType_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.AlarmType add constraint FK_AlarmType_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.AlarmType add constraint FK_AlarmType_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.DayOff add constraint FK_DayOff_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.DayOff add constraint FK_DayOff_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.DayOff add constraint FK_DayOff_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.SkillType add constraint FK_SkillType_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.SkillType add constraint FK_SkillType_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.ScheduleRestriction add constraint FK_ScheduleRestriction_PersonRestriction1 foreign key (Parent) references PersonRestriction
alter table dbo.ScheduleRestriction add constraint FK_ScheduleRestriction_PersonRestriction2 foreign key (PersonRestrictionMain) references PersonRestriction
alter table dbo.ScheduleRestriction add constraint FK_ScheduleRestriction_PersonRestriction3 foreign key (RestrictionPartParent) references PersonRestriction
alter table dbo.PreferenceRestriction add constraint FK_PreferenceRestriction_ScheduleRestriction foreign key (ScheduleRestriction) references ScheduleRestriction
alter table dbo.PreferenceRestriction add constraint FK_PreferenceRestriction_ShiftCategory foreign key (ShiftCategory) references ShiftCategory
alter table dbo.PreferenceRestriction add constraint FK_PreferenceRestriction_Activity foreign key (Activity) references Activity
alter table dbo.ShiftCategory add constraint FK_ShiftCategory_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.ShiftCategory add constraint FK_ShiftCategory_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.ShiftCategory add constraint FK_ShiftCategory_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.AvailabilityRestriction add constraint FK_AvailabilityRestriction_ScheduleRestriction foreign key (ScheduleRestriction) references ScheduleRestriction
alter table dbo.PersonRestriction add constraint FK_PersonRestriction_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PersonRestriction add constraint FK_PersonRestriction_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonRestriction add constraint FK_PersonRestriction_Person3 foreign key (Person) references Person
alter table dbo.PersonRestriction add constraint FK_PersonRestriction_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.KpiTarget add constraint FK_KpiTarget_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.KpiTarget add constraint FK_KpiTarget_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.KpiTarget add constraint FK_KeyPerformanceIndicatorCollection_KeyPerformanceIndicator foreign key (KeyPerformanceIndicator) references KeyPerformanceIndicator
alter table dbo.KpiTarget add constraint FK_KpiTarget_Team foreign key (Team) references Team
alter table dbo.KpiTarget add constraint FK_KpiTarget_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.DayOffPlannerRules add constraint FK_DayOffPlannerRules_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.DayOffPlannerRules add constraint FK_DayOffPlannerRules_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.DayOffPlannerRules add constraint FK_DayOffPlannerRules_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.PersonAccount add constraint FK_PersonAccount_Person foreign key (Parent) references Person
alter table dbo.PersonAccountDay add constraint FK_PersonAccountDay_PersonAccount foreign key (PersonAccount) references PersonAccount
alter table dbo.AvailabilityDayRestriction add constraint FK_AvailabilityDayRestriction_AvailabilityDay foreign key (Parent) references AvailabilityDay
alter table dbo.RotationRestriction add constraint FK_RotationRestriction_ScheduleRestriction foreign key (ScheduleRestriction) references ScheduleRestriction
alter table dbo.RotationRestriction add constraint FK_RotationRestriction_ShiftCategory foreign key (ShiftCategory) references ShiftCategory
alter table dbo.TemplateMultisitePeriod add constraint FK_TemplateMultisitePeriod_MultisiteDayTemplate foreign key (Parent) references MultisiteDayTemplate
alter table dbo.TemplateMultisitePeriodDistribution add constraint FK_TemplateMultisitePeriodDistribution_TemplateMultisitePeriod foreign key (Parent) references TemplateMultisitePeriod
alter table dbo.TemplateMultisitePeriodDistribution add constraint FK_TemplateMultisitePeriodDistribution_ChildSkill foreign key (ChildSkill) references ChildSkill
alter table dbo.SystemRoleApplicationRoleMapper add constraint FK_SystemRoleApplicationRoleMapper_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.SystemRoleApplicationRoleMapper add constraint FK_SystemRoleApplicationRoleMapper_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.SystemRoleApplicationRoleMapper add constraint FK_SystemRoleApplicationRoleMapper_ApplicationRole foreign key (ApplicationRole) references ApplicationRole
alter table dbo.SystemRoleApplicationRoleMapper add constraint FK_SystemRoleApplicationRoleMapper_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.TemplateSkillDataPeriod add constraint FK_TemplateSkillDataPeriod_SkillDayTemplate foreign key (Parent) references SkillDayTemplate
alter table dbo.PartTimePercentage add constraint FK_PartTimePercentage_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PartTimePercentage add constraint FK_PartTimePercentage_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PartTimePercentage add constraint FK_PartTimePercentage_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.PersonCompensation add constraint FK_PersonCompensation_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PersonCompensation add constraint FK_PersonCompensation_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonCompensation add constraint FK_PersonCompensation_Person3 foreign key (Person) references Person
alter table dbo.PersonCompensation add constraint FK_PersonCompensation_Scenario foreign key (Scenario) references Scenario
alter table dbo.PersonCompensation add constraint FK_PersonCompensation_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.Compensation add constraint FK_Compensation_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Compensation add constraint FK_Compensation_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Compensation add constraint FK_Compensation_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.PersonDayOff add constraint FK_PersonDayOff_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PersonDayOff add constraint FK_PersonDayOff_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonDayOff add constraint FK_PersonDayOff_Person3 foreign key (Person) references Person
alter table dbo.PersonDayOff add constraint FK_PersonDayOff_Scenario foreign key (Scenario) references Scenario
alter table dbo.PersonDayOff add constraint FK_PersonDayOff_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.PersonAssignment add constraint FK_PersonAssignment_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PersonAssignment add constraint FK_PersonAssignment_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonAssignment add constraint FK_PersonAssignment_Person3 foreign key (Person) references Person
alter table dbo.PersonAssignment add constraint FK_PersonAssignment_Scenario foreign key (Scenario) references Scenario
alter table dbo.PersonAssignment add constraint FK_PersonAssignment_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.ActivityTimeLimiter add constraint FK_ActivityTimeLimiter_Activity foreign key (Activity) references Activity
--alter table dbo.ActivityTimeLimiter add constraint KF7D95DFD6 foreign key (Parent) references WorkShiftRuleSet
alter table dbo.PersonGroup add constraint FK_PersonGroup_Person foreign key (Person) references Person
alter table dbo.PersonGroup add constraint FK_PersonGroup_PersonGroupBase foreign key (PersonGroup) references PersonGroupBase
alter table dbo.RootPersonGroup add constraint FK_RootPersonGroup_PersonGroupBase foreign key (PersonGroupBase) references PersonGroupBase
alter table dbo.RootPersonGroup add constraint FK_RootPersonGroup_GroupPage foreign key (Parent) references GroupPage
alter table dbo.ChildPersonGroup add constraint FK_ChildPersonGroup_PersonGroupBase1 foreign key (PersonGroupBase) references PersonGroupBase
alter table dbo.ChildPersonGroup add constraint FK_ChildPersonGroup_PersonGroupBase2 foreign key (Parent) references PersonGroupBase
alter table dbo.ContractSchedule add constraint FK_ContractSchedule_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.ContractSchedule add constraint FK_ContractSchedule_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.ContractSchedule add constraint FK_ContractSchedule_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.TimeActivityLayer add constraint FK_TimeActivityLayer_TimeActivity foreign key (payLoad) references TimeActivity
alter table dbo.TimeActivityLayer add constraint FK_TimeActivityLayer_PersonTimeActivity foreign key (Parent) references PersonTimeActivity
alter table dbo.RtaState add constraint FK_RtaState_RtaStateGroup foreign key (Parent) references RtaStateGroup
alter table dbo.PersonRotation add constraint FK_PersonRotation_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PersonRotation add constraint FK_PersonRotation_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonRotation add constraint FK_PersonRotation_Person3 foreign key (Person) references Person
alter table dbo.PersonRotation add constraint FK_PersonRotation_Rotation foreign key (Rotation) references Rotation
alter table dbo.PersonRotation add constraint FK_PersonRotation_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.Outlier add constraint FK_Outlier_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Outlier add constraint FK_Outlier_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Outlier add constraint FK_Outlier_Workload foreign key (Workload) references Workload
alter table dbo.Outlier add constraint FK_Outlier_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.OutlierDates add constraint FK_OutlierDates_Outlier foreign key (Parent) references Outlier
alter table dbo.MultisiteDayTemplate add constraint FK_MultisiteDayTemplate_Skill foreign key (Parent) references Skill
alter table dbo.AvailableData add constraint FK_AvailableData_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.AvailableData add constraint FK_AvailableData_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.AvailableData add constraint FK_AvailableData_ApplicationRole foreign key (ApplicationRole) references ApplicationRole
alter table dbo.AvailableData add constraint FK_AvailableData_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.AvailablePersonsInApplicationRole add constraint FK_AvailablePersonsInApplicationRole_Person foreign key (AvailablePerson) references Person
alter table dbo.AvailablePersonsInApplicationRole add constraint FK_AvailablePersonsInApplicationRole_AvailableData foreign key (AvailableData) references AvailableData
alter table dbo.AvailableTeamsInApplicationRole add constraint FK_AvailableTeamsInApplicationRole_Team foreign key (AvailableTeam) references Team
alter table dbo.AvailableTeamsInApplicationRole add constraint FK_AvailableTeamsInApplicationRole_AvailableData foreign key (AvailableData) references AvailableData
alter table dbo.AvailableSitesInApplicationRole add constraint FK_AvailableSitesInApplicationRole_Site foreign key (AvailableSite) references Site
alter table dbo.AvailableSitesInApplicationRole add constraint FK_AvailableSitesInApplicationRole_AvailableData foreign key (AvailableData) references AvailableData
alter table dbo.AvailableUnitsInApplicationRole add constraint FK_AvailableUnitsInApplicationRole_BusinessUnit foreign key (AvailableBusinessUnit) references BusinessUnit
alter table dbo.AvailableUnitsInApplicationRole add constraint FK_AvailableUnitsInApplicationRole_AvailableData foreign key (AvailableData) references AvailableData
alter table dbo.PersonAbsence add constraint FK_PersonAbsence_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PersonAbsence add constraint FK_PersonAbsence_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonAbsence add constraint FK_PersonAbsence_Person3 foreign key (Person) references Person
alter table dbo.PersonAbsence add constraint FK_PersonAbsence_Scenario foreign key (Scenario) references Scenario
alter table dbo.PersonAbsence add constraint FK_PersonAbsence_Absence foreign key (PayLoad) references Absence
alter table dbo.PersonAbsence add constraint FK_PersonAbsence_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.Team add constraint FK_Team_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Team add constraint FK_Team_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Team add constraint FK_Team_Site foreign key (Site) references Site
alter table dbo.MainShift add constraint FK_MainShift_PersonAssignment foreign key (Id) references PersonAssignment
alter table dbo.MainShift add constraint FK_MainShift_ShiftCategory foreign key (ShiftCategory) references ShiftCategory
alter table dbo.CommonNameDescriptionSetting add constraint FK_CommonNameDescriptionSetting_Setting foreign key (Setting) references Setting
alter table dbo.Meeting add constraint FK_Meeting_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Meeting add constraint FK_Meeting_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Meeting add constraint FK_Meeting_Person3 foreign key (Organizer) references Person
alter table dbo.Meeting add constraint FK_Meeting_Scenario foreign key (Scenario) references Scenario
alter table dbo.Meeting add constraint FK_Meeting_Activity foreign key (PayLoad) references Activity
alter table dbo.Meeting add constraint FK_Meeting_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.ActivityExtender add constraint FK_ActivityExtender_WorkShiftRuleSet foreign key (Parent) references WorkShiftRuleSet
alter table dbo.ActivityExtender add constraint FK_ActivityExtender_Activity foreign key (ExtendWithActivity) references Activity
alter table dbo.ValidatedVolumeDay add constraint FK_ValidatedVolumeDay_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.ValidatedVolumeDay add constraint FK_ValidatedVolumeDay_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.ValidatedVolumeDay add constraint FK_ValidatedVolumeDay_Workload foreign key (Workload) references Workload
alter table dbo.ValidatedVolumeDay add constraint FK_ValidatedVolumeDay_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.SchedulePeriod add constraint FK_SchedulePeriod_Person foreign key (Parent) references Person
alter table dbo.PersonPeriod add constraint FK_PersonPeriod_Team foreign key (Team) references Team
alter table dbo.PersonPeriod add constraint FK_PersonPeriod_Person foreign key (Parent) references Person
alter table dbo.PersonPeriod add constraint FK_PersonPeriod_RuleSetBag foreign key (RuleSetBag) references RuleSetBag
alter table dbo.PersonPeriod add constraint FK_PersonPeriod_PartTimePercentage foreign key (PartTimePercentage) references PartTimePercentage
alter table dbo.PersonPeriod add constraint FK_PersonPeriod_Contract foreign key (Contract) references Contract
alter table dbo.PersonPeriod add constraint FK_PersonPeriod_ContractSchedule foreign key (ContractSchedule) references ContractSchedule
alter table dbo.ExternalLogOnCollection add constraint FK_ExternalLogOnCollection_ExternalLogOn foreign key (ExternalLogOn) references ExternalLogOn
alter table dbo.ExternalLogOnCollection add constraint HENRY_SKRIV_RATT_FOREIGN_KEY_NAMN_INNAN_DU_CHECKAR_IN foreign key (PersonPeriod) references PersonPeriod
alter table dbo.WorkloadDay add constraint FK_WorkloadDay_WorkloadDayBase foreign key (WorkloadDayBase) references WorkloadDayBase
alter table dbo.WorkloadDay add constraint FK_WorkloadDay_Scenario foreign key (Scenario) references Scenario
alter table dbo.WorkloadDay add constraint FK_WorkloadDay_SkillDay foreign key (Parent) references SkillDay
alter table dbo.WorkloadDay add constraint FK_WorkloadDay_Workload foreign key (Workload) references Workload
alter table dbo.Site add constraint FK_Site_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Site add constraint FK_Site_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Site add constraint FK_Site_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.Skill add constraint FK_Skill_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Skill add constraint FK_Skill_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Skill add constraint FK_Skill_SkillType foreign key (SkillType) references SkillType
alter table dbo.Skill add constraint FK_Skill_Activity foreign key (Activity) references Activity
alter table dbo.Skill add constraint FK_Skill_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.ChildSkill add constraint FK_ChildSkill_Skill foreign key (Skill) references Skill
alter table dbo.ChildSkill add constraint FK_ChildSkill_MultisiteSkill foreign key (ParentSkill) references MultisiteSkill
alter table dbo.GroupPage add constraint FK_GroupPage_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.GroupPage add constraint FK_GroupPage_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.GroupPage add constraint FK_GroupPage_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.Availability add constraint FK_Availability_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Availability add constraint FK_Availability_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Availability add constraint FK_Availability_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.PersonTimeActivity add constraint FK_PersonTimeActivity_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PersonTimeActivity add constraint FK_PersonTimeActivity_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonTimeActivity add constraint FK_PersonTimeActivity_Person3 foreign key (Person) references Person
alter table dbo.PersonTimeActivity add constraint FK_PersonTimeActivity_Scenario foreign key (Scenario) references Scenario
alter table dbo.PersonTimeActivity add constraint FK_PersonTimeActivity_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.TimeActivity add constraint FK_TimeActivity_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.TimeActivity add constraint FK_TimeActivity_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.TimeActivity add constraint FK_TimeActivity_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.GroupingAbsence add constraint FK_GroupingAbsence_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.GroupingAbsence add constraint FK_GroupingAbsence_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.MeetingPerson add constraint FK_MeetingPerson_Person foreign key (Person) references Person
alter table dbo.MeetingPerson add constraint FK_MeetingPerson_Meeting foreign key (Parent) references Meeting
alter table dbo.ExternalLogOn add constraint FK_ExternalLogOn_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.ExternalLogOn add constraint FK_ExternalLogOn_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.MultisiteSkill add constraint FK_MultisiteSkill_Skill foreign key (Skill) references Skill
alter table dbo.Scorecard add constraint FK_Scorecard_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Scorecard add constraint FK_Scorecard_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Scorecard add constraint FK_Scorecard_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.KeyPerformanceIndicatorCollection add constraint FK_KeyPerformanceIndicator_KeyPerformanceIndicator foreign key (KeyPerformanceIndicator) references KeyPerformanceIndicator
alter table dbo.KeyPerformanceIndicatorCollection add constraint FK_KeyPerformanceIndicatorCollection_Scorecard foreign key (Scorecard) references Scorecard
alter table dbo.GroupingActivity add constraint FK_GroupingActivity_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.GroupingActivity add constraint FK_GroupingActivity_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Workload add constraint FK_Workload_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Workload add constraint FK_Workload_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Workload add constraint FK_Workload_Skill foreign key (Skill) references Skill
alter table dbo.Workload add constraint FK_Workload_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.QueueSourceCollection add constraint FK_QueueSourceCollection_Workload foreign key (QueueSource) references QueueSource
alter table dbo.QueueSourceCollection add constraint FK_QueueSourceCollection_QueueSource foreign key (Workload) references Workload
alter table dbo.PersonalShift add constraint FK_PersonalShift_PersonAssignment foreign key (Parent) references PersonAssignment
alter table dbo.RotationDay add constraint FK_RotationDay_Rotation foreign key (Parent) references Rotation
alter table dbo.RequestPart add constraint FK_RequestPart_PersonRequest foreign key (Parent) references PersonRequest
alter table dbo.AbsenceRequest add constraint FK_AbsenceRequest_RequestPart foreign key (RequestPart) references RequestPart
alter table dbo.AbsenceRequest add constraint FK_AbsenceRequest_Absence foreign key (Absence) references Absence
alter table dbo.ShiftTradeRequest add constraint FK_ShiftTradeRequest_RequestPart foreign key (RequestPart) references RequestPart
alter table dbo.ShiftTradeRequest add constraint FK_ShiftTradeRequest_Person foreign key (RequestedPerson) references Person
alter table dbo.TextRequest add constraint FK_TextRequest_RequestPart foreign key (RequestPart) references RequestPart
alter table dbo.OutlierDateProviderBase add constraint FK_OutlierDateProviders_Outlier foreign key (Parent) references Outlier
alter table dbo.MultisitePeriod add constraint FK_MultisitePeriod_MultisiteDay foreign key (Parent) references MultisiteDay
alter table dbo.MultisitePeriodDistribution add constraint FK_MultisitePeriodDistribution_MultisitePeriod foreign key (Parent) references MultisitePeriod
alter table dbo.MultisitePeriodDistribution add constraint FK_MultisitePeriodDistribution_ChildSkill foreign key (ChildSkill) references ChildSkill
alter table dbo.RuleSetBag add constraint FK_RuleSetBag_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.RuleSetBag add constraint FK_RuleSetBag_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.RuleSetBag add constraint FK_RuleSetBag_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.SkillDataPeriod add constraint FK_SkillDataPeriod_SkillDay foreign key (Parent) references SkillDay
alter table dbo.Activity add constraint FK_Activity_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Activity add constraint FK_Activity_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Activity add constraint FK_Activity_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.Activity add constraint FK_Activity_GroupingActivity foreign key (GroupingActivity) references GroupingActivity
alter table dbo.PersonAccountTime add constraint FK_PersonAccountTime_PersonAccount foreign key (PersonAccount) references PersonAccount
alter table dbo.CommonSettings add constraint FK_CommonSettings_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.CommonSettings add constraint FK_CommonSettings_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.CommonSettings add constraint FK_CommonSettings_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.QueueSource add constraint FK_QueueSource_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.QueueSource add constraint FK_QueueSource_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.ChartSetting add constraint FK_ChartSetting_Setting foreign key (Setting) references Setting
alter table dbo.SettingCategory add constraint FK_SettingCategory_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.SettingCategory add constraint FK_SettingCategory_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.SettingCategory add constraint FK_SettingCategory_SettingCategory foreign key (Parent) references SettingCategory
alter table dbo.SettingCategory add constraint FK_SettingCategory_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.AvailabilityDay add constraint FK_AvailabilityDay_Rotation foreign key (Parent) references Availability
alter table dbo.Rotation add constraint FK_Rotation_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Rotation add constraint FK_Rotation_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Rotation add constraint FK_Rotation_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.RotationDayRestriction add constraint FK_RotationDayRestriction_RotationDay foreign key (Parent) references RotationDay
alter table dbo.RotationDayRestriction add constraint FK_RotationDayRestriction_ShiftCategory foreign key (ShiftCategory) references ShiftCategory
alter table dbo.PersonRequest add constraint FK_PersonRequest_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.PersonRequest add constraint FK_PersonRequest_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonRequest add constraint FK_PersonRequest_Person3 foreign key (Person) references Person
alter table dbo.PersonRequest add constraint FK_PersonRequest_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.PersonalShiftActivityLayer add constraint FK_PersonalShiftActivityLayer_Activity foreign key (payLoad) references Activity
alter table dbo.PersonalShiftActivityLayer add constraint FK_PersonalShiftActivityLayer_PersonalShift foreign key (Parent) references PersonalShift
alter table dbo.Person add constraint FK_Person_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Person add constraint FK_Person_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.PersonInApplicationRole add constraint FK_PersonInApplicationRole_ApplicationRole foreign key (ApplicationRole) references ApplicationRole
alter table dbo.PersonInApplicationRole add constraint FK_PersonInApplicationRole_Person foreign key (Person) references Person
alter table dbo.Scenario add constraint FK_Scenario_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.Scenario add constraint FK_Scenario_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.Scenario add constraint FK_Scenario_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.ChartSeriesSetting add constraint FK_ChartSeriesSetting_ChartSetting foreign key (Parent) references ChartSetting
--alter table dbo.ContractTimeLimiter add constraint KFDD5AEFC7 foreign key (Parent) references WorkShiftRuleSet
alter table dbo.OptionalColumn add constraint FK_OptionalColumn_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.OptionalColumn add constraint FK_OptionalColumn_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.OptionalColumn add constraint FK_OptionalColumn_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.MultisiteDay add constraint FK_MultisiteDay_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.MultisiteDay add constraint FK_MultisiteDay_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.MultisiteDay add constraint FK_MultisiteDayTemplateReference_Skill foreign key (TemplateReferenceSkill) references MultisiteSkill
alter table dbo.MultisiteDay add constraint FK_MultisiteDay_MultisiteSkill foreign key (Skill) references Skill
alter table dbo.MultisiteDay add constraint FK_MultisiteDay_Scenario foreign key (Scenario) references Scenario
alter table dbo.MultisiteDay add constraint FK_MultisiteDay_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.WorkloadDayTemplate add constraint FK_WorkloadDayTemplate_WorkloadDayBase foreign key (WorkloadDayBase) references WorkloadDayBase
alter table dbo.WorkloadDayTemplate add constraint FK_WorkloadDayTemplate_Workload foreign key (Parent) references Workload
alter table dbo.ApplicationRole add constraint FK_ApplicationRole_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.ApplicationRole add constraint FK_ApplicationRole_Person_UpdatedBy foreign key (UpdatedBy) references Person
alter table dbo.ApplicationRole add constraint FK_ApplicationRole_BusinessUnit foreign key (BusinessUnit) references BusinessUnit
alter table dbo.ApplicationFunctionInRole add constraint FK_ApplicationFunctionInRole_ApplicationFunction foreign key (ApplicationFunction) references ApplicationFunction
alter table dbo.ApplicationFunctionInRole add constraint FK_ApplicationFunctionInRole_ApplicationRole foreign key (ApplicationRole) references ApplicationRole
alter table dbo.ContractScheduleWeek add constraint FK_ContractScheduleWeek_ContractSchedule foreign key (Parent) references ContractSchedule
alter table dbo.ContractScheduleWeekWorkDays add constraint FK_ContractScheduleWeekWorkDays_ContractScheduleWeek foreign key (ContractScheduleWeek) references ContractScheduleWeek
alter table dbo.BusinessUnit add constraint FK_BusinessUnit_Person_CreatedBy foreign key (CreatedBy) references Person
alter table dbo.BusinessUnit add constraint FK_BusinessUnit_Person_UpdatedBy foreign key (UpdatedBy) references Person

----------------
--David Jonsson
--2008-11-21
--Adding two special FK
--This will be a Diff vs. nHib
--nHib knows that an inheratage, with dual FKs in a Class, will end up in a name conflict and uses default names instead of the given name
----------------
alter table dbo.ActivityTimeLimiter add constraint FK_ActivityTimeLimiter_WorkShiftRuleSet foreign key (Parent) references WorkShiftRuleSet
alter table dbo.ContractTimeLimiter add constraint FK_ContractTimeLimiter_WorkShiftRuleSet foreign key (Parent) references WorkShiftRuleSet

PRINT 'Adding tables. Finished!'
----

SET NOCOUNT OFF
PRINT 'Adding build number to database' 
INSERT INTO dbo.DatabaseVersion(BuildNumber, SystemVersion)   VALUES (11,'7.0.11')
