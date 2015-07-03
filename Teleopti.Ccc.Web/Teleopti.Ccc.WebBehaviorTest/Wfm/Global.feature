﻿Feature: Global
	As a resource planner
	I want to have some global featues

@ignore
Scenario: View help widget
	Given I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to permissions			            | True              |
	And I view forecasting
	And I view permissions
	When I click on the help widget
	Then I should see relevant help
