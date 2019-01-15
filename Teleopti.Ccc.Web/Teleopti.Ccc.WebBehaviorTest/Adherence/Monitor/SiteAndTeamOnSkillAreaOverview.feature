@RTA
Feature: Site And Team On SkillArea Overview
  In order to quicker find what site or team manager to contact when skills belongs to skill area goes critical
  As a real time analyst
  I want to see how many agents with a skill area that are in alarm for sites or teams

  Background:
	Given there is a switch
	And I have a role with full access
	And there is an activity named 'Phone'
	And there is a skill named 'Sales' with activity 'Phone'
	And there is a skill named 'Support' with activity 'Phone'
	And there is a skill named 'Invoice' with activity 'Phone'
	And there is a Skill Area called 'Phone skills' that monitors skills 'Sales, Support'
	And there is a site named 'London'
	And there is a team named 'London Team 1' on site 'London'
	And there is a team named 'London Team 2' on site 'London'
	And there is a site named 'Paris'
	And there is a team named 'Paris Team 1' on site 'Paris'
	And Pierre Baldi has a person period with
	  | Field      | Value        |
	  | Team       | Paris Team 1 |
	  | Start Date | 2016-01-01   |
	  | Skill      | Sales        |
	And Ashley Andeen has a person period with
	  | Field      | Value         |
	  | Team       | London Team 1 |
	  | Start Date | 2016-01-01    |
	  | Skill      | Support       |
	And John King has a person period with
	  | Field      | Value         |
	  | Team       | London Team 2 |
	  | Start Date | 2016-01-01    |
	  | Skill      | Invoice       |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-10-03 08:00 |
	  | End time   | 2016-10-03 17:00 |
	And Ashley Andeen has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-10-03 08:00 |
	  | End time   | 2016-10-03 17:00 |
	And John King has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-10-03 08:00 |
	  | End time   | 2016-10-03 17:00 |
	And there is a rule with
	  | Field       | Value        |
	  | Name        | Not adhering |
	  | Activity    | Phone        |
	  | Phone state | Pause        |
	  | Is alarm    | true         |
	And there is a rule with
	  | Field       | Value    |
	  | Name        | Adhering |
	  | Activity    | Phone    |
	  | Phone state | Ready    |
	  | Is alarm    | false    |

  Scenario: See how many agents with a specific skill area that are in alarm for each site
	Given the time is '2016-10-03 08:01'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets her phone state to 'Pause'
	And 'John King' sets her phone state to 'Ready'
	When I view Real time adherence for skill area 'Phone skills' for sites
	Then I should see site 'Paris' with 1 of 1 agents in alarm
	And I should see site 'London' with 1 of 1 agents in alarm
	Given the time is '2016-10-03 08:05'
	When 'Pierre Baldi' sets his phone state to 'Ready'
	And 'Ashley Andeen' sets her phone state to 'Ready'
	Then I should see site 'Paris' with 0 of 1 agents in alarm
	And I should see site 'London' with 0 of 1 agents in alarm

  Scenario: See how many agents with a specific skill area that are in alarm for each team
	Given the time is '2016-10-03 08:01'
	And 'Ashley Andeen' sets her phone state to 'Ready'
	And 'John King' sets his phone state to 'Pause'
	When I view Real time adherence for skill area 'Phone skills' for teams on site 'London'
	Then I should see team 'London Team 1' with 0 of 1 agents in alarm
	Given the time is '2016-10-03 08:05'
	And 'Ashley Andeen' sets her phone state to 'Pause'
	Then I should see team 'London Team 1' with 1 of 1 agents in alarm