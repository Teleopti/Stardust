'Read all settings from comamnd window
'This script takes 4 params,1 st param=networkDtc,2nd param=DtcAuthenticationMode
'3rd param=EnablexaTransactions,4 th param=Restart DTC

EnableNetworkDTC = WScript.Arguments.Item(0)
DtcAuthenticationMode = WScript.Arguments.Item(1)
EnablexaTransactions=WScript.Arguments.Item(2)
RestartDtc=WScript.Arguments.Item(3)

'create global object

Set objShell = WScript.CreateObject("WScript.Shell")

'If Network DTC is enabled
If EnableNetworkDTC = 1 Then
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\NetworkDtcAccess", 1, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\NetworkDtcAccessInbound", 1, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\NetworkDtcAccessOutbound", 1, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\NetworkDtcAccessTransactions", 1, "REG_DWORD"

ElseIf EnableNetworkDTC = 0 Then
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\NetworkDtcAccess", 0, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\NetworkDtcAccessInbound", 0, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\NetworkDtcAccessOutbound", 0, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\NetworkDtcAccessTransactions", 0, "REG_DWORD"

End If

'Mutual authentication
If DtcAuthenticationMode = 0 Then
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\AllowOnlySecureRpcCalls", 1, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\FallbackToUnsecureRPCIfNecessary", 0, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\TurnOffRpcSecurity", 0, "REG_DWORD"

'Incoming Authentication
ElseIf DtcAuthenticationMode = 1 Then
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\AllowOnlySecureRpcCalls", 0, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\FallbackToUnsecureRPCIfNecessary", 1, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\TurnOffRpcSecurity", 0, "REG_DWORD"

'No authentication
ElseIf DtcAuthenticationMode = 2 Then
	objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\AllowOnlySecureRpcCalls", 0, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\FallbackToUnsecureRPCIfNecessary", 0, "REG_DWORD"
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\TurnOffRpcSecurity", 1, "REG_DWORD"

End If

'ENable Xa transactions
if EnablexaTransactions=0 then
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\XaTransactions", 0, "REG_DWORD"

ElseIf EnablexaTransactions = 1 then
    objShell.RegWrite "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSDTC\Security\XaTransactions", 1, "REG_DWORD"

End if

'restart MSDTC
objShell.Run("net stop msdtc"),1,True
objShell.Run("net start msdtc"),1,True