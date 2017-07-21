--Setting a default value based on last change for person absences. An initial value is always set on absence creation from now.
update PersonAbsence set LastChange=updatedon where LastChange is null
GO