@WFM
Feature: Remove Planned Absence
	As a team leader
	I need to remove or shorten absences for multiple reasons.
	I want an easy way to find the agents
	I need to change and make the changes easily. 

Background: 
	Given I am american
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And there is a contract named 'A contract'
	And there is a contract schedule named 'A contract schedule'
	And there is a part time percentage named 'Part time percentage'
	And there is an activity named 'Phone'
	And there is a shift category named 'Day'
	And there is a rule set with
		| Field          | Value       |
		| Name           | A rule set  |
		| Activity       | Phone       |
		| Shift category | Day         |
	And there are absences
		| Name     | Color    |
		| Name     | Vacation |
		| Vacation | Pink     |
		| Illness  | Blue     |
	And there is a shift bag named 'A shift bag' with rule set 'A rule set'
	And there is a skill named 'A skill' with activity 'Phone'
	And I have a person period with
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Team       | Team green |
	And I have a role with
		| Field                         | Value      |
		| Access to team                | Team green |
		| Access to Wfm MyTeam Schedule | true       |
	And 'John Smith' has a workflow control set publishing schedules until '2016-12-01'
	And 'Bill Gates' has a workflow control set publishing schedules until '2016-12-01'
	And 'Candy Mamer' has a workflow control set publishing schedules until '2016-12-01'
	And 'John Smith' has a person period with
		| Field                | Value                |
		| Shift bag            | A shift bag          |
		| Skill                | A skill              |
		| Team                 | Team green           |
		| Start date           | 2016-01-01           |
		| Contract             | A contract           |
		| Contract schedule    | A contract schedule  |
		| Part time percentage | Part time percentage |
	And 'Bill Gates' has a person period with
		| Field                | Value                |
		| Shift bag            | A shift bag          |
		| Skill                | A skill              |
		| Team                 | Team green           |
		| Start date           | 2016-01-01           |
		| Contract             | A contract           |
		| Contract schedule    | A contract schedule  |
		| Part time percentage | Part time percentage |
	And 'Candy Mamer' has a person period with
		| Field                | Value                |
		| Shift bag            | A shift bag          |
		| Skill                | A skill              |
		| Team                 | Team green           |
		| Start date           | 2016-01-01           |
		| Contract             | A contract           |
		| Contract schedule    | A contract schedule  |
		| Part time percentage | Part time percentage |
	And 'John Smith' has a shift with
		| Field          | Value            |
		| Shift category | Day              |
		| Activity       | Phone            |
		| Start time     | 2016-10-10 09:00 |
		| End time       | 2016-10-10 16:00 |
	And 'Bill Gates' has a shift with
		| Field          | Value            |
		| Shift category | Day              |
		| Activity       | Phone            |
		| Start time     | 2016-10-10 09:00 |
		| End time       | 2016-10-10 16:00 |

Scenario: Could delete absences for an agent
	Given 'John Smith' has an absence with
		| Field      | Value            |
		| Name       | Vacation         |
		| Start time | 2016-10-10 10:00 |
		| End time   | 2016-10-10 11:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John'
	And I click button to search for schedules
	Then I should see schedule with absence 'Vacation' for 'John Smith' displayed
	When I selected agent 'John Smith'
	And I open menu in team schedule
	And I click menu item 'RemoveAbsence' in team schedule
	Then I should see a successful notice
	And I should see schedule with no absence for 'John Smith' displayed

Scenario: Absence deletion should only be enabled when when absence selected
	Given 'John Smith' has an absence with
		| Field      | Value            |
		| Name       | Vacation         |
		| Start time | 2016-10-10 10:00 |
		| End time   | 2016-10-10 11:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John'
	And I click button to search for schedules
	And I should see schedule with absence 'Vacation' for 'John Smith' displayed
	And I open menu in team schedule
	Then I should see 'RemoveAbsence' menu item is disabled

Scenario: Full day absence should be able to delete
	Given 'John Smith' has a full day absence named 'Vacation' on '2016-10-10'	
	And 'Bill Gates' has a full day absence named 'Illness' on '2016-10-10'
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I selected agent 'Bill Gates'
	And I open menu in team schedule
	And I click menu item 'RemoveAbsence' in team schedule
	Then I should see a successful notice

Scenario: Could delete absences for multiple agents
	Given 'John Smith' has an absence with
		| Field      | Value            |
		| Name       | Vacation         |
		| Start time | 2016-10-10 10:00 |
		| End time   | 2016-10-10 11:00 |
	And 'Bill Gates' has an absence with
		| Field      | Value            |
		| Name       | Illness          |
		| Start time | 2016-10-10 10:00 |
		| End time   | 2016-10-10 11:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'green'
	And I click button to search for schedules
	Then I should see schedule with absence 'Vacation' for 'John Smith' displayed
	When I selected agent 'John Smith'
	And I selected agent 'Bill Gates'
	And I open menu in team schedule
	And I click menu item 'RemoveAbsence' in team schedule
	Then I should see a successful notice
