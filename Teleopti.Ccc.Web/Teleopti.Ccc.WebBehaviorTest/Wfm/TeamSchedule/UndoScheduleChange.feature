﻿@WFM
Feature: Undo Schedule Change
	As a team leader
	I need to undo my last schedule change

Background: 
Given I am american
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I have a person period with
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Team       | Team green |
	And I have a role with
	| Field                         | Value          |
	| Name                          | Wfm Team Green |
	| Access to everyone            | True           |
	| Access to Wfm MyTeam Schedule | true           |
	| Add Activity                  | true           |
	And there is a shift category named 'Day'
	And there are activities
	| Name     | Color    |
	| Phone    | Green    |
	| Lunch    | Yellow   |
	| Sales    | Red      |
	| Training | Training |
	And there is a contract named 'A contract'
	And there is a contract schedule named 'A contract schedule'
	And there is a part time percentage named 'Part time percentage'
	And there is a rule set with
	| Field          | Value       |
	| Name           | A rule set  |
	| Activity       | Phone       |
	| Shift category | Day         |	
	And there is a shift bag named 'A shift bag' with rule set 'A rule set'
	And there is a skill named 'A skill' with activity 'Phone'
	And 'John Smith' has a workflow control set publishing schedules until '2016-12-01'
	And 'John Smith' has a person period with
	| Field                | Value                |
	| Shift bag            | A shift bag          |
	| Skill                | A skill              |
	| Team                 | Team green           |
	| Start date           | 2016-01-01           |
	| Contract             | A contract           |
	| Contract schedule    | A contract schedule  |
	| Part time percentage | Part time percentage |

Scenario: Should be able to see enable menu
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I open menu in team schedule
	Then I should see 'Undo' menu is enabled
