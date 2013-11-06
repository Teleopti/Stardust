@ignore
Feature: View activity and absence details
	In order to know what the agents are scheduled to work on
	As a team leader
	I want to see the activity or absence name and the start/end times
	
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
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-10-10 |
	And there are shift categories
	| Name  |
	| Day   |
	And there are activities
	| Name        | Color  |
	| Phone       | Green  |
	| Lunch       | Yellow |
	| Training    | Pink   |
	| Short break | Blue   |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	
Scenario: View activity details in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field                | Value            |
	| Shift category       | Day              |
	| Activity             | Phone            |
	| Start time           | 2013-10-10 08:00 |
	| End time             | 2013-10-10 17:00 |
	| Scheduled activity   | Lunch            |
	| Scheduled start time | 2013-10-10 11:00 |
	| Scheduled end time   | 2013-10-10 12:00 |
	When I view schedules for 'Team green' on '2013-10-10'
	And I select the schedule activity for 'Pierre Baldi' with start time '08:00'
	Then I should see schedule activity details for 'Pierre Baldi' with
	| Field      | Value |
	| Name       | Phone |
	| Start time | 08:00 |
	| End time   | 11:00 |

Scenario: View shift details in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-10-10 08:00 |
	| End time         | 2013-10-10 17:00 |
	| Lunch activity   | Lunch            |
	| Lunch start time | 2013-10-10 11:00 |
	| Lunch end time   | 2013-10-10 12:00 |
	| Break activity   | Short break      |
	| Break start time | 2013-10-10 14:00 |
	| Break end time   | 2013-10-10 14:15 |
	When I view schedules for 'Team green' on '2013-10-10'
	And I select any schedule activity for 'Pierre Baldi'
	And I click description toggle button
	Then I should see schedule shift details for 'Pierre Baldi' with
	| Name        | Color  |
	| Phone       | Green  |
	| Lunch       | Yellow |
	| Short break | Blue   |

Scenario: View activity details in team schedule with night shift from yesterday
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-10-09 22:00 |
	| End time         | 2013-10-10 06:00 |
	| Lunch activity   | Lunch            |
	| Lunch start time | 2013-10-10 01:00 |
	| Lunch end time   | 2013-10-10 02:00 |
	When I view schedules for 'Team green' on '2013-10-10'
	And I select the schedule activity for 'Pierre Baldi' with start time '00:00'
	Then I should see schedule activity details for 'Pierre Baldi' with
	| Field      | Value |
	| Name       | Phone |
	| Start time | 22:00 |
	| End time   | 01:00 |

Scenario: View absence details in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-10-10 08:00 |
	| End time       | 2013-10-10 17:00 |
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-10-10 16:00 |
	| End time   | 2013-10-10 17:00 |
	When I view schedules for 'Team green' on '2013-10-10'
	And I select the schedule activity for 'Pierre Baldi' with start time '16:00'
	Then I should see schedule activity details for 'Pierre Baldi' with
	| Field      | Value    |
	| Name       | Vacation |
	| Start time | 16:00    |
	| End time   | 17:00    |

Scenario: View confidential absence details in team schedule
	Given I have a role with
	| Field                      | Value                 |
	| Name                       | Can View Confidential |
	| Access to team             | Team green            |
	| Access to Anywhere         | true                  |
	| View unpublished schedules | true                  |
	| View confidential          | true                  |
	And there is an absence with
	| Field        | Value           |
	| Name         | Mental disorder |
	| Confidential | true            |
	| Color        | Blue            |
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-10-10 08:00 |
	| End time       | 2013-10-10 17:00 |
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Mental disorder  |
	| Start time | 2013-10-10 16:00 |
	| End time   | 2013-10-10 17:00 |
	When I view schedules for 'Team green' on '2013-10-10'
	And I select the schedule activity for 'Pierre Baldi' with start time '16:00'
	Then I should see schedule activity details for 'Pierre Baldi' with
	| Field      | Value           |
	| Name       | Mental disorder |
	| Start time | 16:00           |
	| End time   | 17:00           |

Scenario: Cannot view confidential absence details in team schedule if not permitted
	Given I have a role with
	| Field                      | Value                    |
	| Name                       | Cannot View Confidential |
	| Access to team             | Team green               |
	| Access to Anywhere         | true                     |
	| View unpublished schedules | true                     |
	| View confidential          | false                    |
	And there is an absence with
	| Field        | Value           |
	| Name         | Mental disorder |
	| Confidential | true            |
	| Color        | Blue            |
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-10-10 08:00 |
	| End time       | 2013-10-10 17:00 |
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Mental disorder  |
	| Start time | 2013-10-10 16:00 |
	| End time   | 2013-10-10 17:00 |
	When I view schedules for 'Team green' on '2013-10-10'
	And I select the schedule activity for 'Pierre Baldi' with start time '16:00'
	Then I should see schedule activity details for 'Pierre Baldi' with
	| Field      | Value |
	| Name       | Other |
	| Start time | 16:00 |
	| End time   | 17:00 |




Scenario: View personal activity details in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field                        | Value            |
	| Shift category               | Day              |
	| Activity                     | Phone            |
	| Start time                   | 2013-10-10 08:00 |
	| End time                     | 2013-10-10 17:00 |
	| Lunch activity               | Lunch            |
	| Lunch start time             | 2013-10-10 11:00 |
	| Lunch end time               | 2013-10-10 12:00 |
	| Personal activity            | Training         |
	| Personal activity start time | 2013-10-10 14:00 |
	| Personal activity end time   | 2013-10-10 15:00 |
	When I view schedules for 'Team green' on '2013-10-10'
	And I select the schedule activity for 'Pierre Baldi' with start time '14:00'
	Then I should see schedule activity details for 'Pierre Baldi' with
	| Field       | Value    |
	| Name        | Training |
	| Is Personal | true     |
	| Start time  | 14:00    |
	| End time    | 15:00    |

Scenario: View overtime details in team schedule
	Given I have the role 'Anywhere Team Green'
	And there is a multiplicator definition set named "Overtime After work"
	And 'Pierre Baldi' has a shift with
	| Field                                 | Value               |
	| Shift category                        | Day                 |
	| Activity                              | Phone               |
	| Start time                            | 2013-10-10 08:00    |
	| End time                              | 2013-10-10 17:00    |
	| Lunch activity                        | Lunch               |
	| Lunch start time                      | 2013-10-10 11:00    |
	| Lunch end time                        | 2013-10-10 12:00    |
	| Overtime                              | Phone               |
	| Overtime multiplicator definition set | Overtime After work |
	| Overtime start time                   | 2013-10-10 17:00    |
	| Overtime end time                     | 2013-10-10 18:00    |
	When I view schedules for 'Team green' on '2013-10-10'
	And I select the schedule activity for 'Pierre Baldi' with start time '17:00'
	Then I should see schedule activity details for 'Pierre Baldi' with
	| Field                        | Value               |
	| Name                         | Phone               |
	| Is Overtime                  | true                |
	| Start time                   | 17:00               |
	| End time                     | 18:00               |
	| Multiplicator definition set | Overtime After work |