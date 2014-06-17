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

rem Migrate a Teleopti WFM version 6 db into version 7?
pause
"C:\Program Files\Teleopti\ApplicationConfiguration\CccAppConfig.exe" -SSlocalhost -SDteleopticcc6 -SUsa -SPsapwd -DSlocalhost -DDTeleoptiCCC -DUsa -DPsapwd -TZ"W. Europe Standard Time" -FD2008-01-01 -TD2009-12-31 -BUACME -CO -CUen-GB
rem Finished
pause
