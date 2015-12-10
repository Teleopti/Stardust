@ignore
Feature: SwapShiftsForTwoAgents
	As a team leader
	I want to be able to easily swap shifts between two agents

Scenario: Can only do shift swap when selected 2 agents' schedule
	Given I am a team leader with 'Swap Shifts' permission
	And I am viewing team schedule for '2016-01-01'
	When I selected 3 agents
	Then I should see "Swap shifts" menu item is disabled

Scenario: Could not do shift swap when no permission
	Given I am a team leader without 'Swap Shifts' permission
	And I am viewing team schedule for '2016-01-01'
	When I selected 2 agents
	Then I should not see "Swap shifts" menu item

Scenario: Schedule with full day absence is not allowed to swap
	Given I am a team leader without 'Swap Shifts' permission
	And I am viewing team schedule for '2016-01-01'
	When I selected agent 'Ashley Andeen' with full day absence
	And I selected agent 'Steve Novack' with Early Shift
	Then I should see "Swap shifts" menu item is disabled

Scenario: Schedule with overnight shift is not allowed to swap
	Given I am a team leader without 'Swap Shifts' permission
	And I am viewing team schedule for '2016-01-01'
	When I selected agent 'Ashley Andeen' with overnight shift from yesterday
	And I selected agent 'Steve Novack' with Early Shift
	Then I should see "Swap shifts" menu item is disabled

Scenario: Should see notice that swap shifts finished
	Given I am a team leader without 'Swap Shifts' permission
	And I am viewing team schedule for '2016-01-01'
	When I selected 2 agents
	And I applied 'Swap Shifts'
	Then I should see notice that swap shifts finished successfully
