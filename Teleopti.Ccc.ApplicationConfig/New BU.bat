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
"C:\Data\PBI17968\Teleopti.Ccc.ApplicationConfig\bin\Debug\CccAppConfig.exe" -DSteleopti699 -DDPBI17968_demoreg_TeleoptiCCC7 -DUsa -DPcadadi -TZ"W. Europe Standard Time" -BUTest -CUen-GB
rem Finished
pause
