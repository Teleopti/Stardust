@ignore
Feature: Add activity on a shift
	
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
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-10-29 |
	And 'Ashley Andeen' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-10-29 |
	And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-10-29 |
	And there is a shift category named 'Day'
	And there is an activity with
	| Field | Value  |
	| Name  | Lunch  |
	| Color | Yellow |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	
Scenario: Prototype
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-10-29 07:00 |
	| End time         | 2013-10-29 16:00 |
	| Lunch activity   | Lunch            |
	| Lunch Start time | 2013-10-29 12:00 |
	| Lunch End time   | 2013-10-29 13:00 |
	And 'Ashley Andeen' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-10-29 09:00 |
	| End time         | 2013-10-29 18:00 |
	| Lunch activity   | Lunch            |
	| Lunch Start time | 2013-10-29 13:00 |
	| Lunch End time   | 2013-10-29 14:00 |
	And 'John King' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-10-29 10:00 |
	| End time         | 2013-10-29 19:00 |
	| Lunch activity   | Lunch            |
	| Lunch Start time | 2013-10-29 14:00 |
	| Lunch End time   | 2013-10-29 15:00 |
	When I view schedules for '2012-10-29'
