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


Scenario: Write name and time of current activity when it doesn't exist
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 18:00'
	When I view my regional settings
	And I click ASM link
	Then I should see '' as current start time
	And I should see '' as current end time

Scenario: Write name and time of next activity when it doesn't exist
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 18:00'
	When I view my regional settings
	And I click ASM link
	Then I should see '' as next start time
	And I should see '' as next end time

Scenario: Write name and time of next activity
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 10:00'
	When I view my regional settings
	And I click ASM link
	Then I should see Lunch as next activity
	And I should see '11:00' as next start time
	And I should see '12:00' as next end time

Scenario: Write name and time of next activity when current doesn't exist
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01 07:50'
	When I view my regional settings
	And I click ASM link
	Then I should see Phone as next activity
	And I should see '08:00' as next start time
	And I should see '11:00' as next end time