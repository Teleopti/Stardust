Feature: Global
	As a resource planner
	I want to have some global featues

Scenario: Navigate through breadcrumb
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			            | True              |
	And I view forecasting
	And I view Advanced forecasting option
	When I click on the breadcrumb forecasting link
	Then I should see Forecasting
