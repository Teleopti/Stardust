Feature: Global
	As a resource planner
	I want to have some global featues

@ignore
Scenario: View help widget
	Given I have a role with
		| Field                 | Value            |
		| Name                  | Resource Planner |
		| Access to permissions | True             |
	And I view forecasting
	And I view permissions
	When I click on the help widget
	Then I should see relevant help

Scenario: See available business units
	Given I have a role with
	| Field                 | Value |
	| Access to permissions | True  |
	| Access to everyone    | True  |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 2 |
	And there is a role with
	| Field         | Value           |
	| Name          | Role1           |
	| Description   | Role1           |
	| Business Unit | Business Unit 1 |
	And there is a role with
	| Field         | Value           |
	| Name          | Role2           |
	| Description   | Role2           |
	| Business Unit | Business Unit 2 |
	When I view Permissions
	Then I should have available business units with
	| Name            |
	| BusinessUnit    |
	| Business Unit 1 |
	| Business Unit 2 |

@Ignore
Scenario: Select business unit
	Given I have a role with
	| Field                 | Value |
	| Access to permissions | True  |
	| Access to everyone    | True  |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 2 |
	And there is a role with
	| Field         | Value           |
	| Name          | Role1           |
	| Description   | Role1           |
	| Business Unit | Business Unit 1 |
	And there is a role with
	| Field         | Value           |
	| Name          | Role2           |
	| Description   | Role2           |
	| Business Unit | Business Unit 2 |
	When I view Permissions
	Then I should not see a role 'Role1' in the list
	And I should not see a role 'Role2' in the list
	When I pick business unit 'Business Unit 1'
	Then I should see a role 'Role1' in the list
	And I should not see a role 'Role2' in the list
	When I pick business unit 'Business Unit 2'
	Then I should not see a role 'Role1' in the list
	And I should see a role 'Role2' in the list