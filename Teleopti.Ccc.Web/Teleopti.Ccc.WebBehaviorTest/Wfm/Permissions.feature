Feature: Permissions
	As a resource planner
	I want to work on permissions

Scenario: create a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			            | True              |
	When I view Permissions
	And I create a role 'roleA'
	Then I should see a role 'roleA' in the list


