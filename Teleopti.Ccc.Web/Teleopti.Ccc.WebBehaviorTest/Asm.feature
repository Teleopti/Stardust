@ASM
Feature: ASM
	In order to improve adherence
	As an agent
	I want to see my current activities


Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field         | Value            |
	| Name          | No access to ASM |
	| Access To Asm | False            |

	 And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And there is a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 17:00 |
	| Lunch3HoursAfterStart | true             |


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

Scenario: Write all upcoming activities
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 07:00'
	When I view my regional settings
	And I click ASM link
	Then I should see '3' upcoming activities

Scenario: Current activity should be shown
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 16:00'
	When I view my regional settings
	And I click ASM link
	Then I should see Phone as current activity

Scenario: No current activity to show
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 07:00'
	When I view my regional settings
	And I click ASM link
	Then I should not see as current activity

Scenario: Current activity changes
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 11:59'
	When I view my regional settings
	And I click ASM link
	And Current browser time has changed to '2030-01-01 12:00'
	Then I should see Phone as current activity