----------------  
--Name: David Jonsson
--Date: 2011-09-21  
--Desc: New report: Shift Category and Full Day Absences per Day
----------------
INSERT INTO [mart].[report]
(
[report_id]
,[control_collection_id]
,[report_group_id]
,[url]
,[target]
,[report_name]
,[report_name_resource_key]
,[visible]
,[rpt_file_name]
,[text_id]
,[proc_name]
,[help_key]
,[sub1_name]
,[sub1_proc_name]
,[sub2_name]
,[sub2_proc_name]
)
VALUES
(
26,
16,
1,
'~/Selection.aspx?ReportID=26',
'_blank','Shift Category and Full Day Absences per Day',
'ResReportShiftCategoryAndDayAbsencePerDay',
1,
'~/Reports/CCC/report_shift_category_and_day_absences_per_day.rdlc',
1000,
'mart.report_data_shift_cat_and_day_abs_per_day',
'f01_Report_ShiftCategoryAndFullDayAbsencePerDay.html',
'','','',''
)
GO

----------------  
--Name: David Jonsson
--Date: 2011-09-21  
--Desc: a minor performance improvment
----------------
CREATE NONCLUSTERED INDEX IX_language_translation
ON [mart].[language_translation] ([language_id])
INCLUDE ([term_language],[term_english])

GO

----------------  
--Name: David Jonsson
--Date: 2011-09-22
--Desc: #16276: Obsolet Time zones are shown in reports
----------------
--Add column "mark for delete"
ALTER TABLE mart.dim_time_zone ADD
	to_be_deleted bit NULL
GO

UPDATE mart.dim_time_zone
SET to_be_deleted = 0,default_zone = 0
GO

ALTER TABLE mart.dim_time_zone
	ALTER COLUMN to_be_deleted bit NOT NULL
GO

--Make sure "only one default time zone" exists in the table
ALTER TABLE mart.dim_time_zone ADD
	only_one_default_zone AS CASE default_zone WHEN 1 THEN NULL ELSE time_zone_id END

SET ANSI_PADDING ON
CREATE UNIQUE INDEX UIX_only_one_default_zone ON mart.dim_time_zone(only_one_default_zone)
SET ANSI_PADDING OFF
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (335,'7.1.335') 
