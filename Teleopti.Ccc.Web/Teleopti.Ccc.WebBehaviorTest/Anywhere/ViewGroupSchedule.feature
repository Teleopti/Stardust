@ignore
Feature: View group schedule
	In order to know when my colleagues work
	As a team leader
	I want to see the schedules for other groups

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
	And there is a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2013-12-01         |
	And there is a role with
	| Field                      | Value                       |
	| Name                       | Anywhere Team Green And Red |
	| Access to team             | Team green, Team red        |
	| Access to Anywhere         | true                        |
	And there are shift categories
	| Name  |
	| Day   |	
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And Pierre Baldi have the workflow control set 'Published schedule'
	And John Smith have the workflow control set 'Published schedule'
	And 'Pierre Baldi' has a person period with
	| Field                | Value                     |
	| Team                 | Team green                |
	| Start date           | 2013-10-10                |
	| Contract             | 8 hours a day             |
	| Part time percentage | Common PartTimePercentage |
	| Contract schedule    | Common contract schedule  |
	And 'John Smith' has a person period with
	| Field                | Value                     |
	| Team                 | Team red                  |
	| Start date           | 2013-10-10                |
	| Contract             | Common contract           |
	| Part time percentage | Common PartTimePercentage |
	| Contract schedule    | Common contract schedule  |

Scenario: View group picker options
	Given I have the role 'Anywhere Team Green And Red'
	When I view schedules for '2013-10-10'
	Then I should be able to select groups
	| Group                                    |
	| Common Site/Team green                   |
	| Common Site/Team red                     |
	| Kontrakt/Common contract                 |
	| Kontrakt/8 hours a day                   |
	| Kontraktsschema/Common contract schedule |
	| Deltidsprocent/Common PartTimePercentage |

Scenario: View group schedule
	Given I have the role 'Anywhere Team Green And Red'
	And John Smith have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-10-10 08:00 |
	| End time       | 2013-10-10 17:00 |
	And Pierre Baldi have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-10-10 08:00 |
	| End time       | 2013-10-10 17:00 |
	When I view schedules for '2013-10-10'
	And I select group 'Kontrakt/Common contract'	
	Then I should see schedule for 'John Smith'
	Then I should see no schedule for 'Pierre Baldi'

Scenario: Default to my team
	Given I have the role 'Anywhere Team Green And Red'
	Given I have a person period with 
	| Field      | Value      |
	| Team       | Team red   |
	| Start date | 2013-10-10 |
	When I view schedules for '2013-10-10'
	Then The group picker should have 'Common Site/Team red' selected	

Scenario: Default to first option if I have no team
	Given I have the role 'Anywhere Team Green And Red'
	And I do not belong to any team
	When I view schedules for '2013-10-10'
	Then The group picker should have 'Common Site/Team green' selected	

Scenario: Push group schedule changes
	Given I have the role 'Anywhere Team Green And Red'
	And 'Pierre Baldi' have a shift with
    | Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-10-10 08:00 |
	| End time       | 2013-10-10 17:00 |	
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	When I view schedules for '2013-10-10'
	And I select group 'Kontrakt/8 hours a day'
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