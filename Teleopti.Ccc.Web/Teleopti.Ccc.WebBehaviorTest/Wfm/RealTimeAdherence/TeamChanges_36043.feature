﻿Feature: Team changes
	In order to easier find the team leader to blame
	As a real time analyst
	I want agents changing teams calculated correctly

Background:
	Given there is a switch
	And the time is '2016-02-01 00:00:00'
	And rta is ready
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-02-01 |
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Green      |
	 | Start Date | 2016-02-02 |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Activity   | Phone            |
	| Start time | 2016-02-01 08:00 |
	| End time   | 2016-02-01 17:00 |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Activity   | Phone            |
	| Start time | 2016-02-02 08:00 |
	| End time   | 2016-02-02 17:00 |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |

Scenario: Exclude person changed team
	Given the time is '2016-02-01 09:00:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	Then I should see team 'Red' with 1 employees out of adherence
	When 'Pierre Baldi' changes team to 'Green'
	Then I should see team 'Red' with 0 employees out of adherence

Scenario: Exclude person changed team over time
	Given the time is '2016-02-01 09:00:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	Then I should see team 'Red' with 1 employees out of adherence
	When the time is '2016-02-02 09:00:00'
	And I view Real time adherence for teams on site 'Paris'
	Then I should see team 'Red' with 0 employees out of adherence
