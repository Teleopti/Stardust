Feature: View person schedule
	In order to know how a person in my team should work
	As a team leader
	I want to see the schedule for the person
	
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
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2012-12-01 |
	And there are shift categories
	| Name  |
	| Day   |
	| Night |
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
	And 'Pierre Baldi' have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2012-12-02 08:00 |
	| End time         | 2012-12-02 17:00 |
	| Lunch activity   | Lunch            |
	| Lunch start time | 2012-12-02 11:30 |
	| Lunch end time   | 2012-12-02 12:15 |
	When I view person schedule for 'Pierre Baldi' on '2012-12-02'
	Then I should see these shift layers
	| Start time | End time | Color  |
	| 08:00      | 11:30    | Green  |
	| 11:30      | 12:15    | Yellow |
	| 12:15      | 17:00    | Green  |

Scenario: View night shift from yesterday
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Night            |
	| Activity       | Phone            |
	| Start time     | 2012-12-01 20:00 |
	| End time       | 2012-12-02 04:00 |
	When I view person schedule for 'Pierre Baldi' on '2012-12-02'
	Then I should not see any shift

Scenario: View night shift from today
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Night            |
	| Activity       | Phone            |
	| Start time     | 2012-12-02 20:00 |
	| End time       | 2012-12-03 04:00 |
	When I view person schedule for 'Pierre Baldi' on '2012-12-02'
	Then I should see a shift layer with
	| Field      | Value   |
	| Start time | 20:00   |
	| End time   | 1.04:00 |
	| Color      | Green   |

Scenario: View schedule in persons time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'Pierre Baldi' is located in Hawaii
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2012-12-02 08:00 |
	| End time       | 2012-12-02 17:00 |
	When I view person schedule for 'Pierre Baldi' on '2012-12-02'
	Then I should see a shift layer with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Green |
