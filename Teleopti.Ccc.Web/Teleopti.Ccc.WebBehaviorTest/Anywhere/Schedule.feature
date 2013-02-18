Feature: View team schedule
	In order to know how my team should work today
	As a team leader
	I want to see the schedules for the team
	
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
	And 'Pierre Baldi' is a user with
	| Field         | Value        |
	| Terminal date | 2012-12-31   |
	And 'Pierre Baldi' have a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2012-12-01 |
	And there is an activity with
	| Field | Value  |
	| Name  | Lunch  |
	| Color | Yellow |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |

Scenario: View team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a (read model) shift with
	| Field            | Value        |
	| Person           | Pierre Baldi |
	| Date             | 2012-12-02   |
	| Start time       | 08:00        |
	| End time         | 17:00        |
	| Activity         | Phone        |
	| Lunch start time | 11:30        |
	| Lunch end time   | 12:15        |
	| Lunch activity   | Lunch        |
	When I view schedules for '2012-12-02'
	Then I should see schedule for 'Pierre Baldi'
	
Scenario: View team schedule with night shift from yesterday
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a (read model) shift with
	| Field            | Value        |
	| Person           | Pierre Baldi |
	| Date             | 2012-12-02   |
	| Start time       | 20:00        |
	| End time         | 1.04:00      |
	| Activity         | Phone        |
	| Lunch start time | 23:30        |
	| Lunch end time   | 1.00:15      |
	| Lunch activity   | Lunch        |
	When I view schedules for '2012-12-03'
	Then I should see schedule for 'Pierre Baldi'
	
Scenario: View team schedule, no shift
	Given I have the role 'Anywhere Team Green'
	When I view schedules for '2012-12-03'
	Then I should see no schedule for 'Pierre Baldi'

@ignore
Scenario: View schedules in my time zone

@ignore
Scenario: View team selection

@ignore
Scenario: Change team

@ignore
Scenario: Select date

@ignore
Scenario: Select next/previous day

@ignore
Scenario: Select agent
	Given I have the role 'Anywhere Team Green'
	And there is a shift with
	| Field            | Value        |
	| Person           | Pierre Baldi |
	| Date             | 2012-12-02   |
	| Start time       | 08:00        |
	| End time         | 17:00        |
	| Activity         | Phone        |
	| Lunch start time | 11:30        |
	| Lunch end time   | 12:15        |
	| Lunch activity   | Lunch        |
	When I view schedules for '2012-12-02'
	And I select 'Pierre Baldi'
	Then I should see agent schedule for 'Pierre Baldi' on '2012-12-02'

