
Feature: Manage agent schedule in other time zone
	In order to manage a schedule for an agent in another time zone
	As a team leader
	I want to see the time in both my time zone as well as the agent's

Background:
	Given there is a team with
	| Field | Value            |
	| Name  | Team green       |
	And I have a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |
	And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	And there is a shift category named 'Day'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there is an activity with
	| Field | Value  |
	| Name  | Lunch  |
	| Color | Yellow |
	And there is an absence with
	| Field | Value   |
	| Name  | Dentist |
	| Color | Pink    |

Scenario: Timeline for agent in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in 'Istanbul'
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 09:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add activity form for 'John King' in 'Team green' on '2013-11-18'
	Then I should see schedule for 'John King'
	And I should see my time line with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 16:00 |
	And I should see the agent's time line with
	| Field      | Value     |
	| Time zone  | UTC+02:00 |
	| Start time | 09:00     |
	| End time   | 17:00     |

Scenario: Timeline for agent in the same time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in Stockholm
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 09:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add activity form for 'John King' in 'Team green' on '2013-11-18'
	Then I should see schedule for 'John King'
	And I should see my time line with
	| Field      | Value |
	| Start time | 09:00 |
	| End time   | 17:00 |
	And I should not see the agent's timeline



Scenario: Add activity default times for agent in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And the current time is '2013-11-18 12:05'
	And 'John King' is located in 'Istanbul'
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 08:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add activity form for 'John King' in 'Team green' on '2013-11-18'
	And the browser time is '2013-11-18 12:05:00'
	Then I should see add activity times in other time zone with
	| Field      | Value      |
	| Start time | 13:15      |
	| End time   | 14:15      |

Scenario: Add activity times changed for agent in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in 'Istanbul'
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 09:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add activity form for 'John King' in 'Team green' on '2013-11-18'
	And I input these add activity values
	| Field      | Value |
	| Activity   | Lunch |
	| Start time | 12:00 |
	| End time   | 13:00 |
	Then I should see add activity times in other time zone with
	| Field      | Value      |
	| Start time | 13:00      |
	| End time   | 14:00      |
	
Scenario: Add activity times not displayed in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in Stockholm
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 09:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add activity form for 'John King' in 'Team green' on '2013-11-18'
	Then I should not see add activity times in other time zone




	Scenario: Add intraday absence default times for agent in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And the current time is '2013-11-17 12:00'
	And 'John King' is located in 'Istanbul'
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 08:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add intraday absence form for 'John King' in 'Team green' on '2013-11-18'
	And the browser time is '2013-11-17 12:00:00'
	Then I should see add intraday absence times in other time zone with
	| Field      | Value      |
	| Start time | 08:00      |
	| End time   | 09:00      |

	Scenario: Add intraday absence times changed for agent in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in 'Istanbul'
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 09:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add intraday absence form for 'John King' in 'Team green' on '2013-11-18'
	And I input these intraday absence values
	| Field      | Value   |
	| Absence    | Dentist |
	| Start time | 11:00   |
	| End time   | 12:00   |
	Then I should see add intraday absence times in other time zone with
	| Field      | Value      |
	| Start time | 12:00      |
	| End time   | 13:00      |
	
	Scenario: Add intraday absence times not displayed in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in Stockholm
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 09:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add intraday absence form for 'John King' in 'Team green' on '2013-11-18'
	Then I should not see add intraday absence times in other time zone




Scenario: Move activity default time for agent in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And the current time is '2013-11-18 12:05'
	And 'John King' is located in 'Istanbul'
	And 'John King' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-11-18 08:00 |
	| End time         | 2013-11-18 17:00 |
	| Lunch activity   | Lunch            |
	| Lunch start time | 2013-11-18 12:00 |
	| Lunch end time   | 2013-11-18 13:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I select the activity with
	| Field          | Value            |
	| Activity       | Lunch            |
	| Start time     | 2013-11-18 11:00 |
	And I view person schedules move activity form for 'John King' in 'Team green' on '2013-11-18' with selected start minutes of '660'
	Then I should see selected activity time in other time zone with
	| Field      | Value      |
	| Start time | 12:00      |

	Scenario: Move activity time changed for agent in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in 'Istanbul'
	And 'John King' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-11-18 09:00 |
	| End time         | 2013-11-18 17:00 |
	| Lunch activity   | Lunch            |
	| Lunch start time | 2013-11-18 12:00 |
	| Lunch end time   | 2013-11-18 13:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I select the activity with
	| Field          | Value            |
	| Activity       | Lunch            |
	| Start time     | 2013-11-18 11:00 |
	And I view person schedules move activity form for 'John King' in 'Team green' on '2013-11-18' with selected start minutes of '660'
	And I move the activity
	| Field          | Value            |
	| Start time     | 2013-11-18 13:00 |
	Then I should see selected activity time in other time zone with
	| Field      | Value      |
	| Start time | 14:00      |

Scenario: Move activity times not displayed in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in Stockholm
	And 'John King' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-11-18 09:00 |
	| End time         | 2013-11-18 17:00 |
	| Lunch activity   | Lunch            |
	| Lunch start time | 2013-11-18 12:00 |
	| Lunch end time   | 2013-11-18 13:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I select the activity with
	| Field          | Value            |
	| Activity       | Lunch            |
	| Start time     | 2013-11-18 12:00 |
	And I view person schedules move activity form for 'John King' in 'Team green' on '2013-11-18' with selected start minutes of '660'
	Then I should not see move activity time in other time zone




Scenario: Absence to remove is also displayed in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in 'Istanbul'
	And 'John King' has an absence with
	| Field      | Value            |
	| Name       | Dentist          |
	| Start time | 2013-11-18 00:00 |
	| End time   | 2013-11-18 23:59 |
	When I view person schedule for 'John King' in 'Team green' on '2013-11-18'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Dentist          |
	| Start time | 2013-11-17 23:00 |
	| End time   | 2013-11-18 22:59 |
	And I should see absence in other time zone with
	| Field      | Value            |
	| Time zone  | UTC+02:00        |
	| Start time | 2013-11-18 00:00 |
	| End time   | 2013-11-18 23:59 |

Scenario: Absence to remove is not displayed in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in Stockholm
	And 'John King' has an absence with
	| Field      | Value            |
	| Name       | Dentist          |
	| Start time | 2013-11-18 00:00 |
	| End time   | 2013-11-18 23:59 |
	When I view person schedule for 'John King' in 'Team green' on '2013-11-18'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Dentist          |
	| Start time | 2013-11-18 00:00 |
	| End time   | 2013-11-18 23:59 |
	And I should not see absence in other time zone 