Feature: 22851 - Find Shift Trade Page from Team Schedule
	As an agent I need to easily find the shift trade page,
	so that I find where to look for trade candidates.
	
Background:
	Given there is a team with
	| Field | Value       |
	| Name  | Team green  |
	And there is a role with
	| Field                          | Value      |
	| Name                           | Agent      |
	| Access to team                 | Team green |
	| Access to team schedule        | true       |
	| Access To shift trade requests | true       |
	And there is a role with
	| Field                          | Value                 |
	| Name                           | No shift trade access |
	| Access to team                 | Team green            |
	| Access to team schedule        | true                  |
	| Access To shift trade requests | false                 |

Scenario: Initialize a shift trade from team schedule
	Given I have the role 'Agent'
	And I am viewing team schedule for '2013-10-07'
	When I initialize a shift trade
	Then I should see the shift trade page for '2013-10-07'

Scenario: Can not see shift trade button if no permission
	Given I have the role 'No shift trade access'
	When I view team schedule for '2013-10-07'
	Then I should not be able to initialize a shift trade
