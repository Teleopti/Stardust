@RTA
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
	And Ashely Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-01-01 |
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-01-01 |
	And Ashely Andeen has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-01-14 08:00 |
	  | End time   | 2016-01-14 17:00 |
	And Ashely Andeen has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-01-15 08:00 |
	  | End time   | 2016-01-15 17:00 |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-01-14 08:00 |
	  | End time   | 2016-01-14 17:00 |
	And there is a rule with
	  | Field       | Value |
	  | Adherence   | Out   |
	  | Activity    | Phone |
	  | Phone state | Pause |
	  | Is Alarm    | true  |
	And there is a rule with
	  | Field       | Value |
	  | Adherence   | Out   |
	  | Phone state | Pause |
	  | Is Alarm    | true  |

  Scenario: Exclude terminated agents
	Given the time is '2016-01-14 09:00:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Ashely Andeen' sets his phone state to 'Pause'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	Then I should see team 'Red' with 2 agents in alarm
	When the time is '2016-01-15 12:00:00'
	And I view Real time adherence for teams on site 'Paris'
	Then I should see team 'Red' with 1 agents in alarm

  Scenario: Exclude agents terminated retroactively
	Given the time is '2016-01-14 09:00:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Ashely Andeen' sets his phone state to 'Pause'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	Then I should see team 'Red' with 2 agents in alarm
	When 'Pierre Baldi' is updated with
	  | Field         | Value      |
	  | Terminal Date | 2016-01-05 |
	Then I should see team 'Red' with 1 agents in alarm
