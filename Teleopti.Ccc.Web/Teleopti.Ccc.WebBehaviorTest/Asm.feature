﻿Feature: ASM
	In order to improve adherence
	As an agent
	I want to be to see my current activities


Background:
	Given there is a role with
	| Field                    | Value             |
	| Name                     | Access to mytime  |
    And Current time is
	| Field | Value            |
	| Time  | 2001-01-01 12:00 |

Scenario: No permission to ASM module
	Given I am an agent without access to ASM
	When I am viewing week schedule
	Then ASM link should not be visible 

Scenario: Show agent's schedule in popup
	Given I am an agent
	And I have a schededule today between '8' and '17'
	When I am viewing asm gant
	Then I should see a schedule between '8' and '17'