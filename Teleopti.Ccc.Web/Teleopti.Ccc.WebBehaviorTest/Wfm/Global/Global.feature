@WFM
Feature: Global
	As a resource planner
	I want to have some global featues

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
