@RTA
Feature: Top 50 agents
  In order to easier find agents without skill, site, team, excluding states selection
  As a real time analyst
  I want to see top 50 agents for entire Bu

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Team Preferences' on site 'London'
	And I have a role with full access
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2017-08-04 |
	And Ashley Andeen has a person period with
	  | Field      | Value            |
	  | Team       | Team Preferences |
	  | Start Date | 2017-08-04       |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2017-08-04 08:00 |
	  | End time   | 2017-08-04 17:00 |
	And Ashley Andeen has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2017-08-04 08:00 |
	  | End time   | 2017-08-04 17:00 |
	And there is a rule with
	  | Field       | Value |
	  | Adherence   | In    |
	  | Activity    | Phone |
	  | Phone state | Ready |
	  | Is alarm    | False |
	And there is a rule with
	  | Field           | Value    |
	  | Adherence       | Out      |
	  | Activity        | Phone    |
	  | Phone state     | Pause    |
	  | Is alarm        | True     |
	  | Alarm threshold | 00:01:00 |

  Scenario: See current states for top 50 agents
	Given the time is '2017-08-04  08:00:00'
	And I view real time adherence for all agents on entire Bu
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And 'Ashley Andeen' sets his phone state to 'Ready'
	When the time is '2017-08-04  08:10:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	When the time is '2017-08-04 08:11:00'
	And 'Ashley Andeen' sets his phone state to 'Pause'
	When the time is '2017-08-04 08:15:00'
	Then I should see an agent 'Pierre Baldi'
	And I should see an agent 'Ashley Andeen'
