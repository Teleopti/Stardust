--=========================
--Please uncomment add make manual config here:
:SETVAR TeleoptiAnalytics demo_TeleoptiAnalytics
:SETVAR TeleoptiCCCAgg demo_TeleoptiCCCAgg
--=========================

SET NOCOUNT ON

--Declare
DECLARE @myTeleoptiAnalytics varchar(100)
DECLARE @myTeleoptiCCCagg varchar(100)

DECLARE @TeleoptiAnalytics varchar(100)
DECLARE @TeleoptiCCCagg varchar(100)

--Init
--This is the orgiginal names, do NOT edit this section!
SET @TeleoptiAnalytics			= 'TeleoptiAnalytics'
SET @TeleoptiCCCagg				= 'TeleoptiCCCAgg'

SET @myTeleoptiAnalytics		= '$(TeleoptiAnalytics)'
SET @myTeleoptiCCCagg			= '$(TeleoptiCCCAgg)'

--Switch context
USE $(TeleoptiAnalytics)

--config sys_crossdatabaseview_target
EXEC mart.sys_crossdatabaseview_target_update @TeleoptiAnalytics, @myTeleoptiAnalytics
EXEC mart.sys_crossdatabaseview_target_update @TeleoptiCCCAgg, @myTeleoptiCCCAgg

--deploy all cross database views
EXEC $(TeleoptiAnalytics).mart.sys_crossdatabaseview_load