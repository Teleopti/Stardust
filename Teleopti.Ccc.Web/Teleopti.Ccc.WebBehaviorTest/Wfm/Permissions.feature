﻿Feature: Permissions
	As a resource planner
	I want to work on permissions

@ignore
Scenario: create a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			            | True              |
	When I view Permissions
	And I create a role 'roleA'
	Then I should see a role 'roleA' in the list

@ignore
Scenario: add a function to a role
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			            | True              |
	And there is a role with
		| Field                                 | Value             |
		| Name                                  | RoleA							|
		| Description		                        | RoleA							|
		| Access to permissions			            | True              |
	When I view Permissions
	And I select a role 'RoleA'
	And I select a permission called 'Allt'
	Then I should see 'Allt' selected in the list
