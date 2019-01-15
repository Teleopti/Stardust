@RTA
Feature: Deleted agents
  In order to easier find the team leader to blame
  As a real time analyst
  I do not want to see deleted agents

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-01-01 |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-01-14 08:00 |
	  | End time   | 2016-01-14 17:00 |
	And Ashley Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-01-01 |
	And Ashley Andeen has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-01-14 08:00 |
	  | End time   | 2016-01-14 17:00 |
	And there is a rule with
	  | Field       | Value |
	  | Adherence   | Out   |
	  | Activity    | Phone |
	  | Phone state | Pause |
	  | Is alarm    | true  |

  Scenario: Exclude deleted agents
	Given the time is '2016-01-14 09:00:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets his phone state to 'Pause'
	Then I should see team 'Red' with 2 employees out of adherence
	When 'Pierre Baldi' is deleted
	Then I should see team 'Red' with 1 employees out of adherence
