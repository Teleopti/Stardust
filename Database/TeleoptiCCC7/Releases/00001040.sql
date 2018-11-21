insert into PlanningGroupSettings 
(id, MinDayOffsPerWeek, MaxDayOffsPerWeek, MinConsecutiveWorkDays, MaxConsecutiveWorkDays, 
MinConsecutiveDayOffs, MaxConsecutiveDayOffs, DefaultSettings, Name, Parent, 
BlockFinderType, BlockSameStartTime, BlockSameShiftCategory, BlockSameShift, 
Priority, MinFullWeekendsOff, MaxFullWeekendsOff, MinWeekendDaysOff, MaxWeekendDaysOff)
select newid(), 1, 3, 2, 6, 1, 3, 1, 'Default', pg.id, 0, 0, 0, 0, -1, 0, 8, 0, 16
from PlanningGroup pg
where not exists(select 1 from PlanningGroupSettings pgs where pgs.parent=pg.Id and DefaultSettings=1)



