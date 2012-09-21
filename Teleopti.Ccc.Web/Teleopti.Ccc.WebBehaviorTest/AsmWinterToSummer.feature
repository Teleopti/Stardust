﻿Feature: ASM Winter to Summer
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
	And there is a shift with
	| Field                 | Value            |
	| StartTime             | 2020-03-28 23:00 |
	| EndTime               | 2020-03-29 05:00 |
	#Shift here just to have "one hour length" to compare with when asserting length of layer
	And there is a shift with 
	| Field                 | Value            |
	| StartTime             | 2020-03-28 05:00 |
	| EndTime               | 2020-03-28 06:00 |

Scenario: Shift crossing winter to summer daylight should have correct length
	Given I have the role 'Full access to mytime'
	And Current time is '2020-03-28 20:00'
	When I view my regional settings
	And I click ASM link
	Then The last layer should be '5' hours long