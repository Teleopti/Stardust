@WatiN
Feature: ASM Summer to Winter
	In order to improve adherence
	As an agent
	I want to see my current activities

Background:
	Given I am located in Stockholm
	And there is a role with
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
	And there are shift categories
	| Name  |
	| Night  |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2020-10-24 23:00 |
	| EndTime               | 2020-10-25 05:00 |
	| Shift category		| Night	           |

Scenario: Shift crossing winter to summer daylight should have correct length
	Given I have the role 'Full access to mytime'
	And I am located in Stockholm
	And the current time is '2020-10-24 20:00'
	When I view ASM
	Then The last layer should be '7' hours long
