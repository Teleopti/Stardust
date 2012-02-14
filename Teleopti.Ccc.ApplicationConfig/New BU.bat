::-------------------------------------------------------
::"-SS":   // Source Server Name.
::"-SD":   // Source Database Name.
::"-SU":   // Source User Name.
::"-SP":   // Source Password.
::"-DS":   // Destination Server Name.
::"-DD":   // Destination Database Name.
::"-DU":   // Destination User Name.
::"-DP":   // Destination Password.
::"-TZ":   // TimeZone.
::"-FD":   // Date From.
::"-TD":  // Date To.
::"-BU":  // BusinessUnit Name.
::"-CO":  // Convert.
::"-CU":  // Culture.
::"-DR": // Force merge of Default Resolution to n.
::"-EE":
::"-OM": // Only run merge of default resolution.
::-------------------------------------------------------

rem Create new Business Unit?
"C:\Program Files\Teleopti\ApplicationConfiguration\CccAppConfig.exe" -DSlocalhost -DDTeleoptiCCC -DUteleopticcc -DPteleopticcc -TZ"W. Europe Standard Time" -BUACME -CUen-GB
rem Finished
pause
