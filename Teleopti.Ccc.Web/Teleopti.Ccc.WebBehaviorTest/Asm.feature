Feature: ASM
	In order to improve adherence
	As an agent
	I want to be to see my current activities


Background:
	Given there is a role with
	| Field                    | Value             |
	| Name                     | Access to mytime  |
	| Access to mobile reports | false             |
	And there is a shift with
	| Field             | Value   |
	| StartTime         | 2001-01-01 08:00   |
	| EndTime           | 2001-01-01 17:00   |
	| ShiftCategoryName | ForTest |

Scenario: No permission to ASM module
	Given I have the role 'No access to ASM'
	When I am viewing week schedule
	Then ASM link should not be visible 

Scenario: Show part of agent's schedule in popup
	Given I have the role 'Access to mytime'
	And Current time is '2001-01-01'	
	When I am viewing asm gant
	Then I should see a schedule in popup