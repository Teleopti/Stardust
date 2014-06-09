﻿Feature: Move activity
	As a team leader
	I want to move an activity for one agent

Background:
	Given there is a shift category named 'Day'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |

@ignore
Scenario: The team leader should be able to move an activity
	Given 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules
	And I move the activity
	| Field          | Value            |
	| Agent          | John King        |
	| Date           | 2013-11-18       |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 12:00 |
	And I save
	Then I should see 
	| Field | Value     |
	| Agent | John King |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 12:00 |
	| End time       | 2013-11-18 18:00 |

