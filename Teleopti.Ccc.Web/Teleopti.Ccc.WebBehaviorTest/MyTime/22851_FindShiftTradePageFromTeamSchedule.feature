Feature: 22851 - Find Shift Trade Page from Team Schedule
	As an agent I need to easily find the shift trade page,
	so that I find where to look for trade candidates.

Scenario: Initialize a shift trade from team schedule
	Given I am an agent in a team with access to the whole site
	And I am viewing team schedule for '2013-10-07'
	When I initialize a shift trade
	Then I should navigate to shift trade for '2013-10-07'

Scenario: Can not see shift trade button if no permission
	Given I am an agent in a team without access to shift trade requests
	When I view team schedule
	Then I should not be able to initialize a shift trade


