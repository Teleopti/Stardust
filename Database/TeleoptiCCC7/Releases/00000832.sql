SET NOCOUNT ON

update ApplicationFunction set FunctionCode = 'ViewStaffingInfo', FunctionDescription = 'xxViewStaffingInfo' where ForeignId =  '0143' and FunctionCode = 'ViewPossibility' and FunctionDescription = 'xxViewPossibility'

SET NOCOUNT OFF
GO
