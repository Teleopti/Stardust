@OnlyRunIfEnabled('WfmTeamSchedule_AddActivity_37541')
Feature: AddActivity
	As a team leader
	I want to reassign my agents to a different task by add activity

Background:
	Given there is a team with
	| Field | Value            |
	| Name  | Team green       |
	And I have a role with
	| Field                         | Value          |
	| Name                          | Wfm Team Green |
	| Access to team                | Team green     |
	| Access to Wfm MyTeam Schedule | true           |
	| Add Activity                  | true           |
	And 'Ashley Andeen' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-01-18 |
	And there is a shift category named 'Day'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there is an activity with
	| Field | Value  |
	| Name  | Lunch  |
	| Color | Yellow |

Scenario: Happy Path
	When I am view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-01-01'
	And I selected agent 'Ashley Andeen'
	And I open 'AddActivity' panel
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
Scenario: Should not see add activity menu item without permission
	Given I am a team leader without 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected 1 agents
	Then I should not see 'Add Activity' menu item

@ignore
Scenario: Should see disabled menu item when no agent is selected
	Given I am a team leader with 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected 0 agents
	Then I should see 'Add Activity' menu item is disabled

@ignore
Scenario: Cannot apply new activity when data is invalid
	Given I am a team leader with 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected 1 agents
	And I open 'Add Activity' panel for add activity
	And I set new activity as
	| Field         | Value |
	| Activity type | Phone |
	| Start time    | 12:00 |
	| End time      | 11:00 |
	| Is next day   | false |
	Then I should not apply my new activity

@ignore
Scenario: Can apply new activity when data is valid
	Given I am a team leader with 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected 1 agents
	And I open 'Add Activity' panel for add activity
	And I set new activity as
	| Field         | Value |
	| Activity type | Phone |
	| Start time    | 12:00 |
	| End time      | 13:00 |
	| Is next day   | false |
	Then I should be able to apply my new activity

@ignore
Scenario: Should see notification after applying new activity
	Given I am a team leader with 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected 1 agents
	And I open 'Add Activity' panel for add activity
	And I set new activity as
	| Field         | Value |
	| Activity type | Phone |
	| Start time    | 12:00 |
	| End time      | 13:00 |
	| Is next day   | false |
	And I apply my new activity
	Then I should see a successful notice

@ignore
Scenario: Default activity start time should be the latest schedule start time of selected agents when selected date is not today
	Given I am a team leader with 'Add Activity' permission
	And the time is '2016-01-01 12:00'
	And 'Ashley Andeen' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2016-01-02 09:00 |
	| End time       | 2016-01-02 17:00 |
	And 'Steve Novack' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2016-01-02 10:30 |
	| End time       | 2016-01-02 17:00 |
	When I am viewing team schedule for '2016-01-02'
	And I selected agent 'Ashley Andeen'
	And I selected agent 'Steve Novack'
	And I open 'Add Activity' panel
	Then I should see the add activity form with
	| Field      | Value |
	| Start time | 10:30 |
	| End time   | 11:30 |

@ignore
Scenario: Default activity start time should be next quarter time when selected date is today
	Given I am a team leader with 'Add Activity' permission
	And the time is '2016-01-02 12:10'
	And 'Ashley Andeen' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2016-01-02 09:00 |
	| End time       | 2016-01-02 17:00 |
	And 'Steve Novack' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2016-01-02 10:30 |
	| End time       | 2016-01-02 17:00 |
	When I am viewing team schedule for '2016-01-02'
	And I selected agent 'Ashley Andeen'
	And I selected agent 'Steve Novack'
	And I open 'Add Activity' panel
	Then I should see the add activity form with
	| Field      | Value |
	| Start time | 12:15 |
	| End time   | 13:15 |

@ignore
Scenario: can see failed notice when failing to add activity
	Given I am a team leader with 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected 1 agents
	And I open 'Add Activity' panel for add activity
	And I set new activity as
	| Field         | Value |
	| Activity type | Phone |
	| Start time    | 12:00 |
	| End time      | 13:00 |
	| Is next day   | false |
	And the data source connection is lost
	And I apply my new activity
	Then I should see a failure notice