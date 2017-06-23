@WFM
Feature: ActivityActions
	As a team leader
	I want to modify agent's activities

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
	| Add Personal Activity         | true           |
	| Remove Activity               | true           |
	| Move Activity                 | true           |
	And there is a shift category named 'Day'
	And there are activities
	| Name     | Color    | Allow meeting |
	| Phone    | Green    | true          |
	| Lunch    | Yellow   | false         |
	| Sales    | Red      | true          |
	| Training | Training | true          |
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

Scenario: Should be able to add activity
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John Smith'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I open menu in team schedule
	And I click menu item 'AddActivity' in team schedule
	And I set new activity as
	| Field        | Value      |
	| Activity     | Lunch      |
	| SelectedDate | 2016-10-10 |
	| StartTime     | 2016-10-10 12:00 |
	| EndTime       | 2016-10-10 13:00 |
	| Is next day   | false            |
	Then I should be able to apply my new activity
	When I apply my new activity
	Then I should see a successful notice

@OnlyRunIfEnabled('WfmTeamSchedule_AddPersonalActivity_37742')
Scenario: Should see enabled add personal activity button
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John Smith'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I open menu in team schedule
	Then I should see 'AddPersonalActivity' menu is enabled

@OnlyRunIfEnabled('WfmTeamSchedule_AddPersonalActivity_37742')
Scenario: Should be able to add personal activity
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John Smith'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I open menu in team schedule
	And I click menu item 'AddPersonalActivity' in team schedule
	And I set new activity as
	| Field        | Value            |
	| Activity     | Training         |
	| SelectedDate | 2016-10-10       |
	| StartTime    | 2016-10-10 12:00 |
	| EndTime      | 2016-10-10 13:00 |
	| Is next day  | false            |
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
	And I set schedule date to '2016-10-10'
	And I open menu in team schedule
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
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John Smith'
	And I click button to search for schedules
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
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John Smith'
	And I click button to search for schedules
	And I selected activity 'Training'
	And I selected activity 'Sales'
	And I apply remove activity
	Then I should see a successful notice

#just keep it as information
@ignore 
@OnlyRunIfEnabled('WfmTeamSchedule_RemoveActivity_37743')
Scenario: Should not be able to remove basic activity
	Given 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John Smith'
	And I click button to search for schedules
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
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John Smith'
	And I click button to search for schedules
	And I selected activity 'Lunch'
	And I move activity to '2016-10-10 14:00' with next day being 'false'
	Then I should see a successful notice

@OnlyRunIfEnabled('WfmTeamSchedule_MoveActivity_37744')
Scenario: Should be able to move basic activity
	Given 'John Smith' has a shift with
    | Field            | Value            |
    | Shift category   | Day              |
    | Activity         | Phone            |
    | StartTime        | 2016-10-10 09:00 |
    | EndTime          | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John Smith'
	And I click button to search for schedules
	And I selected activity 'Phone'
	And I move activity to '2016-10-10 10:00' with next day being 'false'
	Then I should see a successful notice

@OnlyRunIfEnabled('WfmTeamSchedule_MoveActivity_37744')
Scenario: The default new start time should be one hour later than the original start time
	Given 'John Smith' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| StartTime      | 2016-10-10 09:00 |
	| EndTime        | 2016-10-10 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John Smith'
	And I click button to search for schedules
	And I selected activity 'Phone'
	And I open menu in team schedule
	And I click menu item 'MoveActivity' in team schedule
	Then I should see the start time to move to is '2016-10-10T10:00'