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
	And I move the activity
	| Field          | Value            |
	| Agent          | John King        |
	| Date           | 2013-11-18       |
	| Activity       | Lunch            |
	| Start time     | 2013-11-18 12:00 |
	| End time     | 2013-11-18 13:00 |
	And I save the shift
	Then I should see schedule activity details for 'John King' with
	| Field      | Value |
	| Name       | Lunch |
	| Start time | 12:00 |
	| End time   | 13:00 |

