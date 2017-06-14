@WFM
Feature: Permissions
	As a resource planner
	I want to work on permissions

Scenario: Create a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			        | True              |
	When I view new Permissions
	And I create a new role 'RoleA'
	Then I should see a role 'RoleA' 

Scenario: Add a function to a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			        | True              |
	And there is a function named 'FunctionA'
	When  I view new Permissions
	And I create a new role 'RoleA'
	And I select function 'FunctionA'
	Then I should see the function selected in the list 'FunctionA'

	Scenario: Remove a function from a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			        | True              |
	And there is a function named 'FunctionA'
	When  I view new Permissions
	And I create a new role 'RoleA'
 	And I select function 'FunctionA'
 	And I select function 'FunctionA'
	Then I should not see 'FunctionA' selected
