@RTA
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
	  | Start Date | 2016-01-01 |
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-01-01 |
	And Ashley Andeen has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-11-23 08:00 |
	  | End time   | 2016-11-23 17:00 |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-11-23 08:00 |
	  | End time   | 2016-11-23 17:00 |
	And there is a rule with
	  | Field           | Value    |
	  | Adherence       | Out      |
	  | Activity        | Phone    |
	  | Phone state     | Training |
	  | Is alarm        | True     |
	  | Alarm threshold | 00:00:00 |
	And there is a rule with
	  | Field           | Value     |
	  | Adherence       | Out       |
	  | Activity        | Phone     |
	  | Phone state     | LoggedOut |
	  | Is alarm        | True      |
	  | Alarm threshold | 00:00:00  |

  Scenario: Hide logged out agents
	Given the time is '2016-11-22 17:00:00'
	And 'Ashley Andeen' sets her phone state to 'LoggedOut'
	And the time is '2016-11-23 08:10:00'
	And 'Pierre Baldi' sets his phone state to 'Training'
	When I am viewing real time adherence for agents without state 'LoggedOut' on team 'Red'
	Then I should not see agent 'Ashley Andeen'
	And I should see agent status for 'Pierre Baldi'