Feature: Time Out Of Adherence
	In order to get a clear picture of who to blame
	As a real time analyst
	I do want to see how long an agent has been out of adherence

Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-06-08 |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2016-06-08 08:00 |
	| End time                 | 2016-06-08 17:00 |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | LoggedOut    |
	| Name            | Not adhering |
	| Is alarm        | true         |
	| Alarm threshold | 00:02:00     |

@OnlyRunIfEnabled('RTA_TotalOutOfAdherenceTime_38702')
Scenario: See time out of adherence
	Given the time is '2016-06-08 09:00:00'
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	Given the time is '2016-06-08 09:10:00'
	When I view real time adherence for all agents on team 'Red'
	Then I should see agent status
	| Field                 | Value        |
	| Name                  | Pierre Baldi |
	| Time out of adherence | 0:10:00      |