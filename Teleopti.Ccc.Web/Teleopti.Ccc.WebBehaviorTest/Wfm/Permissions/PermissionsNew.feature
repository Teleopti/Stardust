@WFM
@OnlyRunIfEnabled('WfmPermission_RelaseRefactor_43587')
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

Scenario: Delete a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			        | True              |
	When I view new Permissions
	And I create a new role 'RoleA'
	And I delete a role 'RoleA'
	Then I should not see role 'RoleA'

Scenario: Edit a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			        | True              | 
	When I view new Permissions
	And I create a new role 'RoleA'
	And I edit the name of role 'RoleA' and write 'RoleB'
	Then I should see a role 'RoleB'

Scenario: Copy a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			        | True              | 
	When I view new Permissions
	And I create a new role 'RoleA'
	And I copy role 'RoleA' 
	Then I should see a role 'Kopia av RoleA'


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

Scenario: Add an organization to a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			        | True              |
	And there is a site named 'site1'
	And there is a team named 'team1' on 'site1'
	When I view new Permissions
	And I create a new role 'RoleA'
	And I select organization selection 'team1'
	Then I should see organization 'team1' selected
	
Scenario: Remove an organization from a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			        | True              |
	And there is a site named 'site1'
	And there is a team named 'team1' on 'site1'
	When I view new Permissions
	And I create a new role 'RoleA'
	And I select organization selection 'team1'
	And I select organization selection 'team1'
	Then I should not see organization 'team1' selected