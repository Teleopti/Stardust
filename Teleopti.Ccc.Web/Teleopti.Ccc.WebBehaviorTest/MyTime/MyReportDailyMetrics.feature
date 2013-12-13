Feature: MyReport Daily Metrics
	In order to improve my own performance
	As an agent
	I need to see metrics for a specific day
	
Scenario: MyReport tab
	Given I am an agent
	When I am viewing an application page
	Then MyReport tab should be visible 

Scenario: No permission to MyReport module
	Given there is a role with
	| Field						| Value						|
	| Name						| No access to MyReport		|
	| Access to MyReport		| False					    |
	And I have the role 'No access to MyReport'
	When I am viewing preferences
	Then MyReport tab should not be visible 

@ignore
Scenario: Open MyReport shows yesterdays figures
	Given I am an agent
	And I view my week schedule for date '2013-10-03'
	And the current time is '2013-10-03'
	And I click MyReport tab
	Then I should see MyReport for '2013-10-02'





