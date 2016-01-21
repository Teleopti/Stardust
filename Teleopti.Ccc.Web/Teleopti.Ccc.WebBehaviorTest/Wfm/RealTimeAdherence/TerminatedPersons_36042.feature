Feature: Terminated agents
	In order to easier find the team leader to blame
	As a real time analyst
	I do not want to see terminated agents

Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And 'Pierre Baldi' is a user with
	| Field         | Value      |
	| Terminal Date | 2016-01-14 |
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-01-01 |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Activity   | Phone            |
	| Start time | 2016-01-14 08:00 |
	| End time   | 2016-01-14 17:00 |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |

@OnlyRunIfEnabled('RTA_TerminatedPersons_36042')
Scenario: Exclude terminated agents
	Given the time is '2016-01-14 09:00:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	Then I should see team 'Red' with 1 employees out of adherence
	When the time is '2016-01-15 12:00:00'
	And I view Real time adherence for teams on site 'Paris'
	Then I should see team 'Red' with 0 employees out of adherence

@OnlyRunIfEnabled('RTA_TerminatedPersons_36042')
Scenario: Exclude agents terminated retroactively
	Given the time is '2016-01-14 09:00:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	Then I should see team 'Red' with 1 employees out of adherence
	When 'Pierre Baldi' is updated with
	| Field         | Value      |
	| Terminal Date | 2016-01-05 |
	Then I should see team 'Red' with 0 employees out of adherence
