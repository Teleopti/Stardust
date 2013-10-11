@ignore
Feature: View group schedule
	In order to know when my colleagues work
	As a team leader
	I want to see team schedule for other groups on any group page
	
Background:
	Given there is a team with
	| Field | Value       |
	| Name  | Team green  |
	| Site  | Common Site |
	And there is a team with
	| Field | Value       |
	| Name  | Team red    |
	| Site  | Common Site |
	And there is a contract with
	| Field                     | Value         |
	| Name                      | 8 hours a day |
	| Average work time per day | 8:00          |
	And there is a role with
	| Field              | Value               |
	| Name               | Anywhere Team Green |
	| Access to team     | Team green          |
	| Access to Anywhere | true                |
	And there is a role with
	| Field              | Value                       |
	| Name               | Anywhere Team Green And Red |
	| Access to team     | Team green, Team red        |
	| Access to Anywhere | true                        |
	And there is a role with
	| Field              | Value           |
	| Name               | Anywhere My Own |
	| Access to my own   | true            |
	| Access to Anywhere | true            |
	And there is a role with
	| Field              | Value                   |
	| Name               | Cannot View Unpublished |
	| Access to team     | Team green              |
	| Access to Anywhere | true                    |
	And there is a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2024-12-25         |
	And there is a workflow control set with
	| Field                      | Value                      |
	| Name                       | Schedule published to 0809 |
	| Schedule published to date | 2013-08-09                 |
	And there is a workflow control set with
	| Field                      | Value                      |
	| Name                       | Schedule published to 0810 |
	| Schedule published to date | 2013-08-10                 |
	And there are shift categories
	| Name  |
	| Day   |
	| Night |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And I have the workflow control set 'Published schedule'
	And Pierre Baldi have the workflow control set 'Published schedule'
	And John Smith have the workflow control set 'Published schedule'
	And I have a person period with 
	| Field                | Value                     |
	| Team                 | Team green                |
	| Start date           | 2013-03-25                |
	| Contract             | Common contract           |
	| Part time percentage | Common PartTimePercentage |
	| Contract schedule    | Common contract schedule  |
	And 'Pierre Baldi' has a person period with
	| Field                | Value                     |
	| Team                 | Team green                |
	| Start date           | 2013-03-25                |
	| Contract             | 8 hours a day             |
	| Part time percentage | Common PartTimePercentage |
	| Contract schedule    | Common contract schedule  |
	And 'John Smith' has have a person period with
	| Field                | Value                     |
	| Team                 | Team red                  |
	| Start date           | 2013-03-25                |
	| Contract             | Common contract           |
	| Part time percentage | Common PartTimePercentage |
	| Contract schedule    | Common contract schedule  |

#new
Scenario: View group selection
	Given I have the role 'Anywhere Team Green And Red'
	When I view schedules for '2013-03-25'
	Then I should be able to select teams
	| Team                                     |
	| Common Site/Team green                   |
	| Common Site/Team red                     |
	| Kontrakt/Common contract                 |
	| Kontrakt/8 hours a day                   |
	| Kontraktsschema/Common contract schedule |
	| Deltidsprocent/Common PartTimePercentage |

#new
Scenario: View group schedule
	Given I have the role 'Anywhere Team Green And Red'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 09:00 |
	| EndTime        | 2013-03-25 18:00 |
	| Shift category | Day              |
	And John Smith have a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 10:00 |
	| EndTime        | 2013-03-25 19:00 |
	| Shift category | Day              |
	And Pierre Baldi have a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 11:00 |
	| EndTime        | 2013-03-25 20:00 |
	| Shift category | Day              |
	When I view schedules for '2013-03-25'
	And I select team 'Kontrakt/Common contract'
	Then I should see schedule for me
	Then I should see schedule for 'John Smith'
	Then I should see no schedule for 'Pierre Baldi'

#new
Scenario: Default to my team
	Given I have the role 'Anywhere Team Green And Red'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 09:00 |
	| EndTime        | 2013-03-25 18:00 |
	| Shift category | Day              |
	When I view team schedule for '2013-03-25'
	Then The team picker should have 'Common Site/Team green' selected
	And I should see schedule for me

#new
Scenario: Default to first option if no my team
	Given I have the role 'Anywhere Team Green And Red'
	And I donot belong to any team
	When I view team schedule for '2013-03-25'
	Then The team picker should have 'Common Site/Team green' selected

Scenario: View my schedule when only have my own data access
	Given I have the role 'Anywhere My Own'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 09:00 |
	| EndTime        | 2013-03-25 18:00 |
	| Shift category | Day              |
	When I view team schedule for '2013-03-25'
	Then I should see schedule for me

Scenario: View group schedule in my time zone
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

Scenario: View group schedule with night shift from yesterday
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field                     | Value            |
	| Shift category            | Night            |
	| Activity                  | Phone            |
	| Start time                | 2013-09-20 20:00 |
	| End time                  | 2013-09-21 04:00 |
	When I view schedules for '2013-09-21'
	Then I should see schedule for 'Pierre Baldi'

Scenario: View group schedule, no shift
	Given I have the role 'Anywhere Team Green'
	When I view schedules for '2013-09-20'
	Then I should see no schedule for 'Pierre Baldi'

Scenario: Change group
	Given I have the role 'Anywhere Team Green And Red'
	When I view schedules for 'Common Site/Team green' on '2013-03-25'
	And I select team 'Common Site/Team red'
	Then I should see person 'John Smith'

Scenario: Select date
	Given I have the role 'Anywhere Team Green'
	When I view schedules for '2013-03-25'
	And I select date '2013-03-26'
	Then I should be viewing schedules for '2013-03-26'

Scenario: Select person
	Given I have the role 'Anywhere Team Green'
	When I view schedules for '2013-03-25'
	And I click person 'Pierre Baldi'
	Then I should be viewing person schedule for 'Pierre Baldi' on '2013-03-25'

Scenario: Only view published schedule
	Given I have the role 'Cannot View Unpublished'
	And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-07-25 |
	And 'John King' has the workflow control set 'Schedule published to 0809'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-08-10 08:00 |
	| End time       | 2013-08-10 17:00 |
	And 'John King' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-08-10 08:00 |
	| End time       | 2013-08-10 17:00 |
	When I view schedules for '2013-08-10'
	Then I should see 'Pierre Baldi' with schedule
	And I should see 'John King' with no schedule 

Scenario: View unpublished schedule when permitted
	Given I have the role 'Anywhere Team Green'
	And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-07-25 |
	And 'John King' has the workflow control set 'Schedule published to 0809'
	And 'John King' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-08-10 08:00 |
	| End time       | 2013-08-10 17:00 |
	When I view schedules for '2013-08-10'
	Then I should see 'John King' with schedule

Scenario: Push group schedule changes
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
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-09-10 00:00 |
	| End time   | 2013-09-11 00:00 |
	Then I should see 'Pierre Baldi' with the schedule
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Red   |
