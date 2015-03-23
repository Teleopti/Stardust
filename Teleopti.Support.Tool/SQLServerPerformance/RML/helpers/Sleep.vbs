Dim Arg
Dim Minutes
Dim Seconds
Dim intTime
Seconds = 1000
Minutes = Seconds * 60

Arg = WScript.Arguments(0) 'input is minutes
WScript.echo "Sleeping for: " & Arg & " minutes..."
intTime = Arg * Minutes 'conver

wscript.sleep intTime