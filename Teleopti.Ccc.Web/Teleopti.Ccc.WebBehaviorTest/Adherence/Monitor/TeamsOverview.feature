@RTA
Feature: Teams overview
  In order to easier find the team leader to blame
  As a real time analyst
  I want to see how many agents that are in alarm for each site

  Background:
	Given there is a switch
	And I have a role with full access
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Green      |
	  | Start Date | 2016-01-01 |
	And Ashley Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-01-01 |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Start time | 2016-08-18 12:00 |
	  | End time   | 2016-08-18 19:00 |
	  | Activity   | Phone            |
	And Ashley Andeen has a shift with
	  | Field      | Value            |
	  | Start time | 2016-08-18 12:00 |
	  | End time   | 2016-08-18 19:00 |
	  | Activity   | Phone            |
	And there is a rule with
	  | Field       | Value |
	  | Activity    | Phone |
	  | Phone state | Ready |
	  | Is Alarm    | false |
	And there is a rule with
	  | Field       | Value |
	  | Activity    | Phone |
	  | Phone state | Pause |
	  | Is Alarm    | true  |

  Scenario: See how many agents that are in alarm for each team
	Given the time is '2016-08-18 13:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets her phone state to 'Ready'
	When I view Real time adherence for teams on site 'Paris'
	Then I should see team 'Green' with 1 of 1 agents in alarm
	And I should see team 'Red' with 0 of 1 agents in alarm

  Scenario: See updates of how many agents that are in alarm for each team
	Given the time is '2016-08-18 13:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets her phone state to 'Ready'
	Then I should see team 'Green' with 1 of 1 agents in alarm
	And I should see team 'Red' with 0 of 1 agents in alarm
