----------------  
--Name: Micke D
--Date: 2012-11-14
--Desc: Purge old personal settings
----------------  
  DELETE FROM dbo.PersonalSettingData
  WHERE [Key]= 'AdvancedPreferencesPersonalSettings'
  DELETE FROM dbo.PersonalSettingData
  WHERE [Key]= 'SchedulingOptionsAdvancedSettings'
GO