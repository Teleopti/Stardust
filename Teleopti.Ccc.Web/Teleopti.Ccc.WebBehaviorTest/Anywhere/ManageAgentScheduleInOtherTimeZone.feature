﻿@Ignore
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

Scenario: View timeline for agent in other time zone
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
	| Start time | 09:00 |
	| End time   | 17:00 |
	And I should see the agent's time line with
	| Field      | Value      |
	| Time zone  | UTC +02:00 |
	| Start time | 10:00      |
	| End time   | 18:00      |

Scenario: View timeline for agent in the same time zone
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

Scenario: View new activity input time for agent in other time zone
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
	And I should see local hours for the agent's new activity with
	| Field      | Value                 |
	| Start time | 13:00                 |
	| End time   | 14:00                 |
	
Scenario: View new activity input time for agent in the same time zone
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
	And I input these add activity values
	| Field      | Value |
	| Activity   | Lunch |
	| Start time | 12:00 |
	| End time   | 13:00 |
	And I should not see any local hours for the agent's new activity

Scenario: View default times for agent in other time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'John King' is located in 'Istanbul'
	And the current time is '2013-11-18 16:00'
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 08:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add activity form for 'John King' in 'Team green' on '2013-11-18'
	And I should see local hours for the agent's new activity with
	| Field      | Value      |
	| Time zone  | UTC +02:00 |
	| Start time | 18:15      |
	| End time   | 19:15      |

