Feature: View person schedule
	In order to know how a person in my team should work
	As a team leader
	I want to see the schedule for the person
	
Background:
	Given there is a team with
	| Field | Value            |
	| Name  | Team green	   |
	And there is a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |
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

Scenario: View default time line
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has no shift
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-09-27'
	Then I should see the time line with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 16:00 |
	
Scenario: View shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2012-12-02 08:00 |
	| End time         | 2012-12-02 17:00 |
	| Lunch activity   | Lunch            |
	| Lunch start time | 2012-12-02 11:30 |
	| Lunch end time   | 2012-12-02 12:15 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2012-12-02'
	Then I should see these scheduled activities
	| Start time | End time | Color  |
	| 08:00      | 11:30    | Green  |
	| 11:30      | 12:15    | Yellow |
	| 12:15      | 17:00    | Green  |

Scenario: Cannot view schedule when not published
	Given I have the role 'Cannot View Unpublished'
	And 'Pierre Baldi' has the workflow control set 'Schedule published to 0809'
	And 'Pierre Baldi' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-08-10 08:00 |
	| End time         | 2013-08-10 17:00 |
	| Lunch activity   | Lunch            |
	| Lunch start time | 2013-08-10 11:30 |
	| Lunch end time   | 2013-08-10 12:15 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-08-10'
	Then I should not see any shift

Scenario: View unpublished schedule when permitted
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has the workflow control set 'Schedule published to 0809'
	And 'Pierre Baldi' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-08-10 08:00 |
	| End time         | 2013-08-10 17:00 |
	| Lunch activity   | Lunch            |
	| Lunch start time | 2013-08-10 11:30 |
	| Lunch end time   | 2013-08-10 12:15 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-08-10'
	Then I should see these scheduled activities
	| Start time | End time | Color  |
	| 08:00      | 11:30    | Green  |
	| 11:30      | 12:15    | Yellow |
	| 12:15      | 17:00    | Green  |

#Bug 28367
@ignore
Scenario: Do not show night shift from yesterday
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Night            |
	| Activity       | Phone            |
	| Start time     | 2012-12-01 20:00 |
	| End time       | 2012-12-02 04:00 |
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Night            |
	| Activity       | Phone            |
	| Start time     | 2012-12-02 05:00 |
	| End time       | 2012-12-02 08:00 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2012-12-02'
	Then I should see a scheduled activity with
	| Field      | Value |
	| Start time | 05:00 |
	| End time   | 08:00 |
	| Color      | Green |
	And I should not see a scheduled activity with
	| Field      | Value |
	| Start time | 20:00 |
	| End time   | 04:00 |
	| Color      | Green |

Scenario: View night shift from today
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Night            |
	| Activity       | Phone            |
	| Start time     | 2012-12-02 20:00 |
	| End time       | 2012-12-03 04:00 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2012-12-02'
	Then I should see a scheduled activity with
	| Field      | Value   |
	| Start time | 20:00   |
	| End time   | 1.04:00 |
	| Color      | Green   |

Scenario: View schedule in my time zone
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'Pierre Baldi' is located in Hawaii
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2012-12-02 20:00 |
	| End time       | 2012-12-03 04:00 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2012-12-03'
	Then I should see a scheduled activity with
	| Field      | Value |
	| Start time | 07:00 |
	| End time   | 15:00 |
	| Color      | Green |