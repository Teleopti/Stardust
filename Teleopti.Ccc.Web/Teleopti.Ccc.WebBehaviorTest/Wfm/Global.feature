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

@ignore
Scenario: See available business units
	Given I have a role with
	| Field              | Value       |
	| Name               | Team leader |
	| Access to everyone | True        |
	| Access to people   | true        |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 2 |
	When I view people
	And I view available business units
	Then I should see available business units with
	| Name            |
	| BusinessUnit    |
	| Business Unit 1 |
	| Business Unit 2 |

@ignore
Scenario: Select business unit
	Given I have a role with
	| Field              | Value       |
	| Name               | Team leader |
	| Access to everyone | true        |
	| Access to people   | true        |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 2 |
	And there is a site 'Paris' on business unit 'Business Unit 1'
	And there is a site 'London' on business unit 'Business Unit 2'
	And there is a team named 'Red' on site 'Paris'
	And there is a team named 'Green' on site 'London'
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2015-01-21 |
	And John Smith has a person period with
	 | Field      | Value      |
	 | Team       | Green      |
	 | Start Date | 2015-01-21 |
	When I view people
	And I select business unit 'Business Unit 2'
	And I search people with keyword 'organization: "Red" "Green"'
	Then I should not see 'Ashley' in people list
	And I should see 'John' in people list
	When I select business unit 'Business Unit 1'
	And I search people with keyword 'organization: "Red" "Green"'
	Then I should see 'Ashley' in people list
	And I should not see 'John' in people list