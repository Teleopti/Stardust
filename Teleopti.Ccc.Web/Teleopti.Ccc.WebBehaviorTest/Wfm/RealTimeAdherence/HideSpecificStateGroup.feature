Feature: Hide specific state groups
            In order to faster find the agents that can answer the phone
            As a real time analyst
            I want to hide specific state groups 
 
Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Team       | Red        |
	| Start Date | 2015-11-23 |
	And Pierre Baldi has a person period with
	| Field      | Value      |
	| Team       | Red        |
	| Start Date | 2015-11-23 |
	And Ashley Andeen has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2015-11-23 08:00 |
	| End time                 | 2015-11-23 17:00 |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2015-11-23 08:00 |
	| End time                 | 2015-11-23 17:00 |
	And there is a rule with 
	| Field           | Value        |
	| Adherence       | Out          |
	| Activity        | Phone        |
	| Phone state     | Training     |  
	| Is alarm        | True         |
	| Alarm threshold | 00:00:00     |
	And there is a rule with 
	| Field           | Value      |
	| Adherence       | Out        |
	| Activity        | Phone      |
	| Phone state     | LoggedOut  |
	| Is alarm        | True       |
	| Alarm threshold | 00:01:00   |
 
 @ignore
@OnlyRunIfEnabled('RTA_HideAgentsBeingLoggedOut_40469')
Scenario: Hide logged out agents
	Given the time is '2015-11-22 17:00:00'			
	And 'Ashley Andeen' sets her phone state to 'LoggedOut'		
	And I am viewing real time adherence for agents on team 'Red'
	And the time is '2015-11-23 08:10:00'	
	And 'Pierre Baldi' sets his phone state to 'Training'
	When I deselet state group 'LoggedOut'
	Then I should not see agent 'Ashley Andeen'
	And I should see agent status for 'Pierre Baldi'
