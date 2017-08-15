@WFM
@OnlyRunIfDisabled('Wfm_GroupPages_45057')
Feature: SwapShiftsForTwoAgents
	As a team leader
	I want to be able to easily swap shifts between two agents

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
	| Swap Shifts                   | true           |	
	And there is a shift category named 'Day'
	And there are activities
	| Name     | Color    |
	| Phone    | Green    |
	| Lunch    | Yellow   |
	| Sales    | Red      |
	| Training | Training |
	And there are absences
	| Name     | Color |
	| Vacation | Pink  |
	| Illness  | Red   |
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
	And 'Bill Gates' has a workflow control set publishing schedules until '2016-12-01'
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

Scenario: Can swap shifts when selected 2 agents' schedule
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	And 'Bill Gates' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Sales            |
	| StartTime        | 2016-10-10 10:00 |
	| EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I selected agent 'Bill Gates'
	And I open menu in team schedule
	And I click menu item 'SwapShifts' in team schedule
	Then I should see a successful notice

@ignore
Scenario: Could not do shift swap when no permission
	Given 'John Smith' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2016-10-10 08:00 |
	| End time   | 2016-10-10 17:00 |
	And 'Bill Gates' has an absence with
	| Field      | Value            |
	| Name       | Illness          |
	| Start time | 2016-10-10 08:00 |
	| End time   | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I selected agent 'Bill Gates'
	And I open menu in team schedule
	Then I should not see 'SwapShifts' menu item

Scenario: Schedule with full day absence is not allowed to swap
	Given 'John Smith' has a full day absence named 'Vacation' on '2016-10-10'	
	And 'Bill Gates' has a full day absence named 'Illness' on '2016-10-10'
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I selected agent 'Bill Gates'
	And I open menu in team schedule
	Then I should see 'SwapShifts' menu item is disabled

Scenario: Schedule with overnight shift from yesterday is not allowed to swap
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-09 17:00 |
	| EndTime          | 2016-10-10 08:00 |
	And 'Bill Gates' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Sales            |
	| StartTime        | 2016-10-10 08:00 |
	| EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I selected agent 'Bill Gates'
	And I open menu in team schedule
	And I click menu item 'SwapShifts' in team schedule
	Then I should see 'SwapShifts' menu item is disabled