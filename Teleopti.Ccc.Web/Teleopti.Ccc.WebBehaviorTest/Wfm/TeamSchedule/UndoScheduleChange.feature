@WFM
@OnlyRunIfEnabled('WfmTeamSchedule_RevertToPreviousSchedule_39002')
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

@ignore
Scenario: Should be able to undo schedule change
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I open menu in team schedule
	And I click menu item 'AddActivity' in team schedule
	And I set new activity as
	| Field         | Value            |
	| Activity      | Phone            |
	| Selected date | 2016-10-10       |
	| Start time    | 2016-10-10 09:00 |
	| End time      | 2016-10-10 17:00 |
	| Is next day   | false            |
	When I apply my new activity
	And I should see a successful notice
	And I close the success notice
	And I selected agent 'John Smith'
	And I open menu in team schedule
	And I click menu item 'AddActivity' in team schedule
	And I set new activity as
	| Field         | Value            |
	| Activity      | Training         |
	| Selected date | 2016-10-10       |
	| Start time    | 2016-10-10 12:00 |
	| End time      | 2016-10-10 13:00 |
	| Is next day   | false            |
	When I apply my new activity
	And I should see a successful notice
	And I close the success notice
	And I selected agent 'John Smith'
	And I open menu in team schedule
	And I click menu item 'Undo' in team schedule
	Then I should see a successful notice
	And I should not see activity 'Training' in schedule