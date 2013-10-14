@ignore
Feature: View group schedule
	In order to know when my colleagues work
	As a team leader
	I want to see the schedules for other groups

Background:
	Given there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And there is a team named 'Team red' on 'The site'
	And there is a contract named 'A contract'
	And there is a contract named 'Another contract'
	And there is a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2013-12-01         |
	And 'Pierre Baldi' have the workflow control set 'Published schedule'
	And 'John Smith' have the workflow control set 'Published schedule'
	And I have a role with
	| Field              | Value                |
	| Access to team     | Team green, Team red |
	| Access to Anywhere | true                 |

Scenario: View group picker options
	Given there is a contract schedule named 'A contract schedule'
	And there is a part time percentage named 'Part time percentage'
	When I view schedules for '2013-10-10'
	Then I should be able to select groups
	| Group                               |
	| The site/Team green                 |
	| The site/Team red                   |
	| Kontrakt/A contract                 |
	| Kontrakt/Another contract           |
	| Kontraktsschema/A contract schedule |
	| Deltidsprocent/Part time percentage |

Scenario: View group schedule
	And 'John Smith' has a shift on '2013-10-10'
	And 'John Smith' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-10-10 |
	| Contract   | A contract |
	And 'Pierre Baldi' has a shift on '2013-10-10'
	And 'Pierre Baldi' has a person period with
	| Field      | Value            |
	| Team       | Team green       |
	| Start date | 2013-10-10       |
	| Contract   | Another contract |
	Given I have a role with
	| Field              | Value                |
	| Access to team     | Team green, Team red |
	| Access to Anywhere | true                 |
	When I view schedules for '2013-10-10'
	And I select group 'Kontrakt/A contract'
	Then I should see schedule for 'John Smith'
	Then I should see no schedule for 'Pierre Baldi'

Scenario: Default to my team
	Given I have a person period with 
	| Field      | Value      |
	| Team       | Team red   |
	| Start date | 2013-10-10 |
	When I view schedules for '2013-10-10'
	Then the group picker should have 'The site/Team red' selected	

Scenario: Default to first option if I have no team
	Given I have no team
	When I view schedules for '2013-10-10'
	Then the group picker should have 'The site/Team green' selected	
