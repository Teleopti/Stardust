﻿Feature: View team schedule
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
	And there is a role with
	| Field                      | Value           |
	| Name                       | Anywhere My Own |
	| Access to my own           | true            |
	| Access to Anywhere         | true            |
	| View unpublished schedules | true            |
	And there is a role with
	| Field              | Value                   |
	| Name               | Cannot View Unpublished |
	| Access to team     | Team green              |
	| Access to Anywhere | true                    |
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
	And there is a workflow control set with
	| Field                      | Value                      |
	| Name                       | Schedule published to 0809 |
	| Schedule published to date | 2013-08-09                 |
	And there is a workflow control set with
	| Field                      | Value                      |
	| Name                       | Schedule published to 0810 |
	| Schedule published to date | 2013-08-10                 |
	
Scenario: View empty when no team available
	Given I have the role 'Anywhere My Own'
	When I view schedules for '2013-08-10'
	Then I should see no team available

Scenario: View team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field                     | Value            |
	| Shift category            | Day              |
	| Activity                  | Phone            |
	| Start time                | 2012-12-02 08:00 |
	| End time                  | 2012-12-02 17:00 |
	| Lunch activity            | Lunch            |
	| Lunch 3 hours after start | true             |
	When I view schedules for '2012-12-02'
	Then I should see schedule for 'Pierre Baldi'

@ignore
Scenario: View team schedule in my time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Hawaii
	And 'Pierre Baldi' is located in Stockholm
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-09-20 22:00 |
	| End time       | 2013-09-21 05:00 |
	When I view schedules for '2013-09-20'
	Then I should see a shift layer with
	| Field      | Value |
	| Start time | 10:00 |
	| End time   | 17:00 |
	| Color      | Green |

Scenario: View team schedule with night shift from yesterday
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field                     | Value            |
	| Shift category            | Night            |
	| Start time                | 2012-12-02 20:00 |
	| End time                  | 2012-12-03 04:00 |
	| Activity                  | Phone            |
	| Lunch activity            | Lunch            |
	| Lunch 3 hours after start | true             |
	When I view schedules for '2012-12-03'
	Then I should see schedule for 'Pierre Baldi'
	
Scenario: View team schedule, no shift
	Given I have the role 'Anywhere Team Green'
	When I view schedules for '2012-12-03'
	Then I should see no schedule for 'Pierre Baldi'
	
Scenario: View team selection
	Given there is a team with
	| Field | Value      |
	| Name  | Team other |
	And there is a role with
	| Field                      | Value                         |
	| Name                       | Anywhere Team Green And Other |
	| Access to team             | Team green, Team other        |
	| Access to Anywhere         | true                          |
	| View unpublished schedules | true                          |
	And I have the role 'Anywhere Team Green And Other'
	When I view schedules for '2012-12-02'
	Then I should be able to select teams
	| Team       |
	| Team green |
	| Team other |

Scenario: Change team
	Given there is a team with
	| Field | Value      |
	| Name  | Team other |
	And there is a role with
	| Field                      | Value                         |
	| Name                       | Anywhere Team Green And Other |
	| Access to team             | Team green, Team other        |
	| Access to Anywhere         | true                          |
	| View unpublished schedules | true                          |
	And I have the role 'Anywhere Team Green And Other'
	And 'Max Persson' has a person period with
	| Field      | Value      |
	| Team       | Team other |
	| Start date | 2012-12-01 |
	When I view schedules for 'Team green' on '2012-12-02'
	And I select team 'Team other'
	Then I should see person 'Max Persson'

Scenario: Select date
	Given I have the role 'Anywhere Team Green'
	When I view schedules for '2012-12-02'
	And I select date '2012-12-03'
	Then I should be viewing schedules for '2012-12-03'

Scenario: Select person
	Given I have the role 'Anywhere Team Green'
	When I view schedules for '2012-12-02'
	And I click person 'Pierre Baldi'
	Then I should be viewing person schedule for 'Pierre Baldi' on '2012-12-02'

Scenario: Only view published schedule
	Given I have the role 'Cannot View Unpublished'
	And 'John Smith' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2012-12-01 |
	And 'Pierre Baldi' has the workflow control set 'Schedule published to 0810'
	And 'John Smith' has the workflow control set 'Schedule published to 0809'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-08-10 08:00 |
	| End time       | 2013-08-10 17:00 |
	And 'John Smith' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-08-10 08:00 |
	| End time       | 2013-08-10 17:00 |
	When I view schedules for '2013-08-10'
	Then I should see 'Pierre Baldi' with schedule
	And I should see 'John Smith' with no schedule 

Scenario: View unpublished schedule when permitted
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has the workflow control set 'Schedule published to 0809'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-08-10 08:00 |
	| End time       | 2013-08-10 17:00 |
	When I view schedules for '2013-08-10'
	Then I should see 'Pierre Baldi' with schedule
	
@ignore
Scenario: Push team schedule changes
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
    | Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-09-10 08:00 |
	| End time       | 2013-09-10 17:00 |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	When I view schedules for '2013-09-10'
	Then I should see 'Pierre Baldi' with the schedule
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Green |	
	When 'Martin Fowler' adds an absence for 'Pierre Baldi' with
	| Field   | Value        |	
	| Name       | Vacation         |
	| Start time | 2013-09-10 00:00 |
	| End time   | 2013-09-11 00:00 |
	Then I should see 'Pierre Baldi' with the schedule
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Red   |

