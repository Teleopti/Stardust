Feature: ActivityActions
	As a team leader
	I want to modify agent's activities

Background:
	Given I am american
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I have a role with
	| Field                         | Value          |
	| Name                          | Wfm Team Green |
	| Access to everyone            | True           |
	| Access to Wfm MyTeam Schedule | true           |
	| Add Activity                  | true           |
	| Add Personal Activity         | true           |
	| Remove Activity               | true           |
	| Move Activity                 | true           |
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

@OnlyRunIfEnabled('WfmTeamSchedule_AddActivity_37541')
Scenario: Should be able to add activity
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-01-01'
	And I selected agent 'John Smith'
	And I click menu button in team schedule
	And I click menu item 'AddActivity' in team schedule
	And I set new activity as
	| Field       | Value |
	| Activity    | Phone |
	| Start time  | 12:00 |
	| End time    | 13:00 |
	| Is next day | false |
	Then I should be able to apply my new activity
	When I apply my new activity
	Then I should see a successful notice

@ignore
@OnlyRunIfEnabled('WfmTeamSchedule_AddActivity_37541')
Scenario: Default activity start time range should be 08:00-09:00 when agent's schedule is empty
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I selected agent 'John Smith'
	And I click menu item 'AddActivity' in team schedule
	Then I should see the add activity time starts '08:00' and ends '09:00'

@ignore
@OnlyRunIfEnabled('WfmTeamSchedule_AddPersonalActivity_37742')
Scenario: Should see enabled add personal activity button
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I selected agent 'John Smith'
	And I click menu button in team schedule
	Then I should see 'AddPersonalActivity' menu is enabled

@ignore
@OnlyRunIfEnabled('WfmTeamSchedule_AddPersonalActivity_37742')
Scenario: Should be able to add personal activity
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I selected agent 'John Smith'
	And I click menu button in team schedule
	And I click menu item 'AddPersonalActivity' in team schedule
	And I apply add personal activity
	Then I should see a successful notice
	
@OnlyRunIfEnabled('WfmTeamSchedule_RemoveActivity_37743')
Scenario: Should see disabled remove activity button when no activity is selected
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	| Lunch Activity   | Lunch            |
	| Lunch start time | 2016-10-10 12:00 |
	| Lunch end time   | 2016-10-10 13:00 |
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I click menu button in team schedule
	Then I should see 'RemoveActivity' menu item is disabled

@OnlyRunIfEnabled('WfmTeamSchedule_RemoveActivity_37743')
Scenario: Should be able to remove single activity
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	| Lunch Activity   | Lunch            |
	| Lunch start time | 2016-10-10 12:00 |
	| Lunch end time   | 2016-10-10 13:00 |
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I selected activity 'Lunch'
	And I apply remove activity
	Then I should see a successful notice

@OnlyRunIfEnabled('WfmTeamSchedule_RemoveActivity_37743')
Scenario: Should be able to remove multiple activities
	Given 'John Smith' has a shift with
	| Field                         | Value            |
	| Shift category                | Day              |
	| Activity                      | Phone            |
	| StartTime                     | 2016-10-10 08:00 |
	| EndTime                       | 2016-10-10 17:00 |
	| Scheduled activity            | Training         |
	| Scheduled activity start time | 2016-10-10 11:00 |
	| Scheduled activity end time   | 2016-10-10 13:00 |
	| Third Activity                | Sales            |
	| Third activity start time     | 2016-10-10 14:00 |
	| Third activity end time       | 2016-10-10 15:00 |
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I selected activity 'Training'
	And I selected activity 'Sales'
	And I apply remove activity
	Then I should see a successful notice

@OnlyRunIfEnabled('WfmTeamSchedule_RemoveActivity_37743')
@ignore
Scenario: Should not be able to remove basic activity
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I selected activity 'Phone'
	And I apply remove activity
	Then I should see an error notice

@OnlyRunIfEnabled('WfmTeamSchedule_MoveActivity_37744')
Scenario: Should be able to move activity
	Given 'John Smith' has a shift with
    | Field            | Value            |
    | Shift category   | Day              |
    | Activity         | Phone            |
    | StartTime        | 2016-10-10 09:00 |
    | EndTime          | 2016-10-10 17:00 |
    | Lunch Activity   | Lunch            |
    | Lunch start time | 2016-10-10 12:00 |
    | Lunch end time   | 2016-10-10 13:00 |
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I selected activity 'Lunch'
	And I apply move activity
	Then I should see a successful notice

@OnlyRunIfEnabled('WfmTeamSchedule_MoveActivity_37744')
Scenario: Should not be able to move basic activity
	Given 'John Smith' has a shift with
    | Field            | Value            |
    | Shift category   | Day              |
    | Activity         | Phone            |
    | StartTime        | 2016-10-10 09:00 |
    | EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I selected activity 'Phone'
	And I apply move activity
	Then I should see a successful notice