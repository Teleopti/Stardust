@WatiN
Feature: Team schedule for groups
In order to know when my colleagues work
As an agent
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
	| Field                          | Value                          |
	| Name                           | Access to view all group pages |
	| Access to team                 | Team red                       |
	| Access to team schedule        | true                           |
	| Access to view all group pages | true                           |
	And there is a role with
	| Field                          | Value                               |
	| Name                           | Without view group pages permission |
	| Access to team                 | Team red                            |
	| Access to team schedule        | true                                |
	| Access to view all group pages | false                               |
	And there is a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2014-02-25         |
	And there are shift categories
	| Name |
	| Day  |
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
	And Pierre Baldi have a person period with
	| Field                | Value                     |
	| Team                 | Team green                |
	| Start date           | 2013-03-25                |
	| Contract             | 8 hours a day             |
	| Part time percentage | Common PartTimePercentage |
	| Contract schedule    | Common contract schedule  |
	And John Smith have a person period with
	| Field                | Value                     |
	| Team                 | Team red                  |
	| Start date           | 2013-03-25                |
	| Contract             | Common contract           |
	| Part time percentage | Common PartTimePercentage |
	| Contract schedule    | Common contract schedule  |


Scenario: View available custom group options
	Given I have the role 'Access to view all group pages'
	When I view team schedule for '2013-03-25'
	And I open the team-picker
	Then I should see available group options
	| Value                                    |
	| Common Site/Team green                   |
	| Common Site/Team red                     |
	| Kontrakt/Common contract                 |
	| Kontrakt/8 hours a day                   |
	| Kontraktsschema/Common contract schedule |
	| Deltidsprocent/Common PartTimePercentage |

Scenario: View group schedule
	Given I have the role 'Access to view all group pages'
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
	When I view team schedule for '2013-03-25'
	And I select 'Kontrakt/Common contract' in the team picker
	Then I should see my schedule in team schedule with
	| Field     | Value |
	| StartTime | 09:00 |
	| EndTime   | 18:00 |
	And I should see 'John Smith' schedule in team schedule with
	| Field     | Value |
	| StartTime | 10:00 |
	| EndTime   | 19:00 |
	And I should not see 'Pierre Baldi' schedule

Scenario: Sort late shifts after early shifts
	Given I have the role 'Access to view all group pages'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 09:00 |
	| EndTime        | 2013-03-25 18:00 |
	| Shift category | Day              |
	And John Smith have a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 08:00 |
	| EndTime        | 2013-03-25 17:00 |
	| Shift category | Day              |
	When I view team schedule for '2013-03-25'
	And I select 'Kontrakt/Common contract' in the team picker
	Then I should see 'John Smith' before myself

Scenario: Default to my team
	Given I have the role 'Access to view all group pages'
	When I view team schedule for '2013-03-25'
	Then The team picker should have 'Common Site/Team green' selected

Scenario: Keep selected group when changing date
	Given I have the role 'Access to view all group pages'
	When I view team schedule for '2013-03-25'
	And I select 'Kontrakt/8 hours a day' in the team picker
	And I click the next day button
	Then I should see colleague 'Pierre Baldi'
	And I should not see myself

Scenario: Keep selected date when changing group
	Given I have the role 'Access to view all group pages'
	And I am viewing team schedule for tomorrow
	When I select something in the team picker
	Then I should see tomorrow

Scenario: View available team options if not have view all group pages permission
	Given I have the role 'Without view group pages permission'
	When I view team schedule for '2013-03-25'
	And I open the team-picker
	Then I should see available team options
	| Value                  |
	| Common Site/Team green |
	| Common Site/Team red   |