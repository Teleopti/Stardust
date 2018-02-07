update PurgeSetting set Value = Value*12, [Key] = 'MonthsToKeepPayroll'
where [Key] = 'YearsToKeepPayroll'
