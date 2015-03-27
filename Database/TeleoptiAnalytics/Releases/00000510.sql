update rta.ActualAgentState set PlatformTypeId = '00000000-0000-0000-0000-000000000000'
where StateCode = 'CCC Logged out'
and PlatformTypeId <> '00000000-0000-0000-0000-000000000000'
