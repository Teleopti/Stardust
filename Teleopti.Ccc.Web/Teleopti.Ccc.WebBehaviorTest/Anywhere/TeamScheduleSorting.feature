Feature: Team schedule sorting
	In order to easily find who is working at a specific time
	As a team leader
	I want to get the persons sorted in order of the schedules
	
Background:
	Given there is a team with
	| Field | Value      |
	| Name  | Team green |
	And there is a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |
	And 'Ashley Andeen' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-09-26 |
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-09-26 |
	And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-09-26 |
	And 'Pierre Andeen' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-09-26 |
	And there are shift categories
	| Name  |
	| Day   |	
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	And there is a dayoff with
    | Field | Value   |
    | Name  | Day off |

Scenario: Order like shift start, full day absence, day off, no shift 
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-09-27 08:00 |
	| End time       | 2013-09-27 16:00 |
	And 'Ashley Andeen' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-09-27 00:00 |
	| End time   | 2013-09-27 23:59 |
	And 'John King' have a day off with
	| Field | Value      |
	| Name  | Day off    |
	| Date  | 2013-09-27 |
	And 'Pierre Andeen' has no shift
	When I view schedules for 'Team green' on '2013-09-27'
	Then I should see 'Pierre Baldi' before 'Ashley Andeen'
	And I should see 'Ashley Andeen' before 'John King'
	And I should see 'John King' before 'Pierre Andeen'

Scenario: Order by shift start when shift and day off in view
	Given I have the role 'Anywhere Team Green'
	And I am located in Hawaii
	And 'Pierre Baldi' is located in Stockholm
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-09-27 08:00 |
	| End time       | 2013-09-27 17:00 |
	And 'Pierre Baldi' have a day off with
	| Field | Value      |
	| Name  | Day off    |
	| Date  | 2013-09-28 |
	And 'Ashley Andeen' is located in Hawaii
	And 'Ashley Andeen' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-09-27 08:00 |
	| End time       | 2013-09-27 17:00 |
	When I view schedules for 'Team green' on '2013-09-27'
	Then I should see 'Pierre Baldi' before 'Ashley Andeen'

Scenario: Order as full day absence when on day off
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Andeen' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-10-28 15:00 |
	| End time       | 2013-10-28 23:00 |
	And 'Ashley Andeen' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-10-28 14:00 |
	| End time       | 2013-10-28 22:00 |	
	And 'Ashley Andeen' has a full day absence named 'Vacation' on '2013-10-28'	
	And 'Pierre Baldi' has a day off named 'Day off' on '2013-10-28'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-10-28 00:00 |
	| End time   | 2013-10-28 23:59 |
	And 'John King' has a day off named 'Day off' on '2013-10-28'
	When I view schedules for '2013-10-28'
	Then I should see 'Pierre Andeen' before 'Pierre Baldi'
	And I should see 'Pierre Baldi' before 'Ashley Andeen'
	And I should see 'Ashley Andeen' before 'John King'