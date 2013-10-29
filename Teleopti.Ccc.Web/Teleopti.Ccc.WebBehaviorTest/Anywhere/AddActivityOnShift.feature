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
	| Start date | 2013-10-28 |
	And 'Ashley Andeen' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-10-28 |
	And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-10-28 |
	And 'Martin Fowler' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-10-28 |
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
	And 'John King' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-10-28 22:00 |
	| End time         | 2013-10-29 06:00 |
	And 'Pierre Baldi' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-10-29 06:00 |
	| End time         | 2013-10-29 15:00 |
	| Lunch activity   | Lunch            |
	| Lunch Start time | 2013-10-29 11:00 |
	| Lunch End time   | 2013-10-29 12:00 |
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
	And 'Martin Fowler' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-10-29 20:00 |
	| End time         | 2013-10-30 04:00 |
	| Lunch activity   | Lunch            |
	| Lunch Start time | 2013-10-30 00:00 |
	| Lunch End time   | 2013-10-30 01:00 |
	When I view schedules for '2012-10-29'
