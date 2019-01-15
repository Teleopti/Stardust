@RTA
Feature: Agents in organization with skills
  In order to quicker find what site or team manager to contact when a skill goes critical
  As a real time analyst
  I want to see how many agents with that skill that are in alarm for selected team
  
  Background:
	Given there is a switch
	And I have a role with full access
	And there is an activity named 'Phone'
	And there is a skill named 'Phone' with activity 'Phone'
	And there is an activity named 'Email'
	And there is a skill named 'Email' with activity 'Email'
	And there is a Skill Area called 'Phone and Email skills' that monitors skills 'Phone, Email'
	And there is a site named 'London'
	And there is a team named 'London Team 1' on site 'London'
	And there is a site named 'Paris'
	And there is a team named 'Paris Team 1' on site 'Paris'
	And Pierre Baldi has a person period with
	  | Field      | Value        |
	  | Team       | Paris Team 1 |
	  | Start Date | 2016-01-01   |
	  | Skill      | Phone        |
	And Ashley Andeen has a person period with
	  | Field      | Value         |
	  | Team       | London Team 1 |
	  | Start Date | 2016-01-01    |
	  | Skill      | Phone         |
	And John King has a person period with
	  | Field      | Value         |
	  | Team       | London Team 1 |
	  | Start Date | 2016-01-01    |
	  | Skill      | Email         |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-11-03 08:00 |
	  | End time   | 2016-11-03 17:00 |
	And Ashley Andeen has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-11-03 08:00 |
	  | End time   | 2016-11-03 17:00 |
	And John King has a shift with
	  | Field      | Value            |
	  | Activity   | Email            |
	  | Start time | 2016-11-03 08:00 |
	  | End time   | 2016-11-03 17:00 |
	And there is a rule with
	  | Field       | Value        |
	  | Name        | Not adhering |
	  | Activity    | Phone        |
	  | Phone state | Pause        |
	  | Is alarm    | true         |
	And there is a rule with
	  | Field       | Value        |
	  | Name        | Not adhering |
	  | Activity    | Email        |
	  | Phone state | Pause        |
	  | Is alarm    | true         |

  Scenario: See agents with a specific skill that are in alarm for a selected sites
	Given the time is '2016-11-03 08:01'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets her phone state to 'Pause'
	And 'John King' sets her phone state to 'Pause'
	Given the time is '2016-11-03 08:15'
	And I am viewing real time adherence for skill 'Phone' on sites 'London, Paris'
	Then I should see agent status for 'Pierre Baldi'
	And I should see agent status for 'Ashley Andeen'
	And I should not see agent 'John King'

  Scenario: See agents with a specific skill that are in alarm for selected teams
	Given the time is '2016-11-03 08:01'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets her phone state to 'Pause'
	And 'John King' sets her phone state to 'Pause'
	Given the time is '2016-11-03 08:15'
	And I am viewing real time adherence for skill 'Phone' on teams 'London Team 1, Paris Team 1'
	Then I should see agent status for 'Pierre Baldi'
	And I should see agent status for 'Ashley Andeen'
	And I should not see agent 'John King'

  Scenario: See agents with a specific Skill Area that are in alarm for selected sites
	Given the time is '2016-11-03 08:01'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets her phone state to 'Pause'
	And 'John King' sets her phone state to 'Pause'
	Given the time is '2016-11-03 08:15'
	And I am viewing real time adherence for skill area 'Phone and Email skills' on sites 'London, Paris'
	Then I should see agent status for 'Pierre Baldi'
	And I should see agent status for 'Ashley Andeen'
	And I should see agent status for 'John King'

  Scenario: See agents with a specific Skill Area that are in alarm for selected teams
	Given the time is '2016-11-03 08:01'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets her phone state to 'Pause'
	And 'John King' sets her phone state to 'Pause'
	Given the time is '2016-11-03 08:15'
	And I am viewing real time adherence for skill area 'Phone and Email skills' on teams 'London Team 1, Paris Team 1'
	Then I should see agent status for 'Pierre Baldi'
	And I should see agent status for 'Ashley Andeen'
	And I should see agent status for 'John King'