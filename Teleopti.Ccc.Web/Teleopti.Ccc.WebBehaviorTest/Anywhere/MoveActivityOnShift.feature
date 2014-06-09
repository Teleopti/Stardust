Feature: Move activity
	As a team leader
	I want to move an activity for one agent

Background:
	Given there is a shift category named 'Day'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |


Scenario: The team leader should be able to move an activity
	Given 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I move the activity
	| Field          | Value            |
	| Agent          | John King        |
	| Date           | 2013-11-18       |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 12:00 |
	And I save
	Then I should see schedule activity details for 'John King' with
	| Field      | Value |
	| Name       | Phone |
	| Start time | 08:00 |
	| End time   | 11:00 |


