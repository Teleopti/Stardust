@ASM
Feature: ASM
	In order to improve adherence
	As an agent
	I want to see my current activities


Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
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

@ignore
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

Scenario: Write name and time of current activity
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 10:00'
	When I view my regional settings
	And I click ASM link
	Then I should see Phone as current activity
	And I should see '08:00' as current start time
	And I should see '11:00' as current end time

Scenario: Write name and time of next activity
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 10:00'
	When I view my regional settings
	And I click ASM link
	Then I should see Lunch as next activity
	And I should see '11:00' as next start time
	And I should see '12:00' as next end time