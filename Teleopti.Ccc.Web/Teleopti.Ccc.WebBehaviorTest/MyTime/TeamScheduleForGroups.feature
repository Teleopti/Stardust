@WatiN
Feature: Team schedule for groups
In order to know when my colleagues work
As an agent
I want to see team schedule for other groups on any group page
 
Background:
	Given there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And there is a team named 'Team red' on 'The site'
	And there is a contract named 'A contract'
	And there is a contract named 'Another contract'
	And there is a contract schedule named 'A contract schedule'
	And there is a part time percentage named 'A part time percentage'
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
	And there is a shift category named 'Day'
	And I have the workflow control set 'Published schedule'
	And 'Pierre Baldi' has the workflow control set 'Published schedule'
	And 'John Smith' has the workflow control set 'Published schedule'
	And I have a person period with 
	| Field                | Value                  |
	| Team                 | Team green             |
	| Start date           | 2013-03-25             |
	| Contract             | A contract             |
	| Part time percentage | A part time percentage |
	| Contract schedule    | A contract schedule    |
	And 'Pierre Baldi' has a person period with
	| Field                | Value                  |
	| Team                 | Team green             |
	| Start date           | 2013-03-25             |
	| Contract             | Another contract       |
	| Part time percentage | A part time percentage |
	| Contract schedule    | A contract schedule    |
	And 'John Smith' has a person period with
	| Field                | Value                  |
	| Team                 | Team red               |
	| Start date           | 2013-03-25             |
	| Contract             | A contract             |
	| Part time percentage | A part time percentage |
	| Contract schedule    | A contract schedule    |

Scenario: View available custom group options
	Given I have the role 'Access to view all group pages'
	When I view group schedule for '2013-03-25'
	And I open the team-picker
	Then I should see available group options
	| Value                                 |
	| The site/Team green                   |
	| The site/Team red                     |
	| Kontrakt/A contract                   |
	| Kontrakt/Another contract             |
	| Kontraktsschema/A contract schedule   |
	| Deltidsprocent/A part time percentage |

Scenario: View group schedule
	Given I have the role 'Access to view all group pages'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 09:00 |
	| EndTime        | 2013-03-25 18:00 |
	| Shift category | Day              |
	And John Smith has a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 10:00 |
	| EndTime        | 2013-03-25 19:00 |
	| Shift category | Day              |
	And Pierre Baldi has a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 11:00 |
	| EndTime        | 2013-03-25 20:00 |
	| Shift category | Day              |
	When I view group schedule for '2013-03-25'
	And I select 'Kontrakt/A contract' in the team picker
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
	And John Smith has a shift with
	| Field          | Value            |
	| StartTime      | 2013-03-25 08:00 |
	| EndTime        | 2013-03-25 17:00 |
	| Shift category | Day              |
	When I view group schedule for '2013-03-25'
	And I select 'Kontrakt/A contract' in the team picker
	Then I should see 'John Smith' before myself

@ignore
Scenario: Default to my team
	Given I have the role 'Access to view all group pages'
	When I view group schedule for '2013-03-25'
	Then The team picker should have 'The site/Team green' selected

Scenario: Keep selected group when changing date
	Given I have the role 'Access to view all group pages'
	When I view group schedule for '2013-03-25'
	And I select 'Kontrakt/Another contract' in the team picker
	And I click the next day button
	Then I should see colleague 'Pierre Baldi'
	And I should not see myself

Scenario: Keep selected date when changing group
	Given I have the role 'Access to view all group pages'
	And I am viewing group schedule for tomorrow
	When I select something in the team picker
	Then I should see tomorrow

Scenario: View available team options if not have view all group pages permission
	Given I have the role 'Without view group pages permission'
	When I view group schedule for '2013-03-25'
	And I open the team-picker
	Then I should see available team options
	| Value               |
	| The site/Team green |
	| The site/Team red   |