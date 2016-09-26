﻿@Anywhere
Feature: Move activity
	As a team leader
	I want to move an activity for one agent

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
	And there is a shift category named 'Day'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there is an activity with
	| Field | Value  |
	| Name  | Lunch  |
	| Color | Yellow |

@ignore
Scenario: The team leader should be able to move an activity
	Given 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	| Lunch activity | Lunch            |
	| Lunch start time     | 2013-11-18 12:00 |
	| Lunch end time       | 2013-11-18 13:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I select the activity with
	| Field          | Value            |
	| Activity       | Lunch            |
	| Start time     | 2013-11-18 12:00 |
	And I view person schedules move activity form for 'John King' in 'Team green' on '2013-11-18' with selected start minutes of '720'
	And I move the activity
	| Field          | Value            |
	| Start time     | 2013-11-18 13:00 |
	And I save the shift
	Then I should see the moved schedule activity details for 'John King' with
	| Field      | Value |
	| Color      | Yellow |
	| Start time | 13:00 |
	| End time   | 14:00 |

@ignore
Scenario: The team leader should be able to move an activity by drag and drop
	Given 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	| Lunch activity | Lunch            |
	| Lunch start time     | 2013-11-18 12:00 |
	| Lunch end time       | 2013-11-18 13:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I select the activity with
	| Field          | Value            |
	| Activity       | Lunch            |
	| Start time     | 2013-11-18 12:00 |
	And I view person schedules move activity form for 'John King' in 'Team green' on '2013-11-18' with selected start minutes of '720'
	And I move the activity by dnd of '60' minutes
	And I save the shift
	Then I should see the moved schedule activity details for 'John King' with
	| Field      | Value |
	| Color      | Yellow |
	| Start time | 13:00 |
	| End time   | 14:00 |

Scenario: The team leader should be able to change the current selected activity
	Given 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	| Lunch activity | Lunch            |
	| Lunch start time     | 2013-11-18 12:00 |
	| Lunch end time       | 2013-11-18 13:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I select the activity with
	| Field          | Value            |
	| Activity       | Lunch            |
	| Start time     | 2013-11-18 12:00 |
	And I view person schedules move activity form for 'John King' in 'Team green' on '2013-11-18' with selected start minutes of '720'
	And I select the activity with
	| Field          | Value            |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	Then The current selected activity has been changed to
	| Field          | Value            |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |

@ignore
Scenario: Back to viewing schedule after moving an activity
	Given 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	| Lunch activity | Lunch            |
	| Lunch start time     | 2013-11-18 12:00 |
	| Lunch end time       | 2013-11-18 13:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I select the activity with
	| Field          | Value            |
	| Activity       | Lunch            |
	| Start time     | 2013-11-18 12:00 |
	And I view person schedules move activity form for 'John King' in 'Team green' on '2013-11-18' with selected start minutes of '720'
	And I move the activity
	| Field          | Value            |
	| Start time     | 2013-11-18 13:00 |
	And I save the shift
	Then I should be viewing schedules for '2013-11-18'

Scenario: Back to viewing schedule after cancel move activity
	Given 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	| Lunch activity | Lunch            |
	| Lunch start time     | 2013-11-18 12:00 |
	| Lunch end time       | 2013-11-18 13:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I select the activity with
	| Field          | Value            |
	| Activity       | Lunch            |
	| Start time     | 2013-11-18 12:00 |
	And I view person schedules move activity form for 'John King' in 'Team green' on '2013-11-18' with selected start minutes of '720'
	And I click 'cancel'
	Then I should be viewing schedules for '2013-11-18'
