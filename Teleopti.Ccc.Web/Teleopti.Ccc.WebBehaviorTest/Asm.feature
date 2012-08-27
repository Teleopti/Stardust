﻿Feature: ASM
	In order to improve adherence
	As an agent
	I want to see my current activities


Background:
	Given there is a role with
	| Field                    | Value             |
	| Name                     | Full access to mytime  |
	And there is a shift with
	| Field             | Value   |
	| StartTime         | 2030-01-01 08:00   |
	| EndTime           | 2030-01-01 17:00   |
	| ShiftCategoryName | ForTest |

Scenario: No permission to ASM module
	Given I have the role 'No access to ASM'
	When I am viewing week schedule
	Then ASM link should not be visible 

Scenario: Show part of agent's schedule in popup
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01'	
	When I view my week schedule
	And I click ASM link
	Then I should see a schedule in popup