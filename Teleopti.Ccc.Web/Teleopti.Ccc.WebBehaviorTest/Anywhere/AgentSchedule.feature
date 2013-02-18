@ignore
Feature: View agent schedule
	In order to know how an agent in my team should work
	As a team leader
	I want to see the schedule for the agent
	
Background:
	Given there is a team with
	| Field | Value            |
	| Name  | Team green	   |
	And I have a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |
	And 'Pierre Baldi' is a user with
	| Field         | Value        |
	| Terminal date | 2012-12-31   |
	And 'Pierre Baldi' has a person period with
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

Scenario: View shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a (read model) shift with
	| Field          | Value        |
	| Person         | Pierre Baldi |
	| Date           | 2012-12-02   |
	| Start time     | 08:00        |
	| End time       | 17:00        |
	| Activity       | Phone        |
	When I view agent schedule for 'Pierre Baldi' on '2012-12-02'
	Then I should see a shift

Scenario: View lunch
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
	When I view agent schedule for 'Pierre Baldi' on '2012-12-02'
	Then I should see 3 shift layers

Scenario: View night shift from yesterday
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a (read model) shift with
	| Field            | Value        |
	| Person           | Pierre Baldi |
	| Date             | 2012-12-01   |
	| Start time       | 20:00        |
	| End time         | 1.04:00      |
	| Activity         | Phone        |
	| Lunch start time | 23:30        |
	| Lunch end time   | 1.00:15      |
	| Lunch activity   | Lunch        |
	When I view agent schedule for 'Pierre Baldi' on '2012-12-02'
	Then I should not see any shift

Scenario: View night shift from today
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
	When I view agent schedule for 'Pierre Baldi' on '2012-12-02'
	Then I should see 3 shift layers

Scenario: View schedule in agents time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'Pierre Baldi' is located in Hawaii
	And 'Pierre Baldi' have a (read model) shift with
	| Field            | Value        |
	| Person           | Pierre Baldi |
	| Date             | 2012-12-02   |
	| Start time       | 08:00        |
	| End time         | 17:00        |
	| Activity         | Phone        |
	When I view agent schedule for 'Pierre Baldi' on '2012-12-02'
	Then I should see a shift layer with
	| Field      | Value |
	| Start time | 23:00 |
	| End time   | 08:00 |
