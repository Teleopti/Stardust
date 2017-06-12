@WFM
Feature: Planning Period
	As a resource planner
	I want to work on planning periods

Scenario: The first planning period suggestion should be the next upcoming schedule period
	Given the time is '2016-06-07'
	And I am swedish
	And I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to resource planner            | True              |
	And there is a site named 'Site 1'
	And there is a team named 'Team 1' on 'Site 1'
	And Ashley Andeen has a person period with
		| Field      | Value      |
		| Team       | Team 1     |
		| Start Date | 2016-06-05 |
	And Ashley Andeen has a schedule period with
		| Field      | Value      |
		| Start date | 2016-06-05 |
		| Type       | Week       |
		| Length     | 1          |
	And there is an planning group with
		| Field               | Value           |
		| Planning group name | PlanningGroup 1 |
		| Team                | Team 1          |
	When I view planning periods for planning group 'PlanningGroup 1'
	And I click create planning period
	And I click apply planning period
	Then I should see a planning period between '2016-06-12' and '2016-06-18'

	Scenario: Creating next planning period should generate a period with the same type as the previous one 
	Given the time is '2016-06-07'
	And I am swedish
	And I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to resource planner            | True              |
	And there is a site named 'Site 1'
	And there is a team named 'Team 1' on 'Site 1'
	And there is an planning group with
		| Field               | Value           |
		| Planning group name | PlanningGroup 1 |
		| Team                | Team 1          |
	And there is a planning period with
		| Field               | Value           |
		| Date                | 2016-06-01      |
		| Planning group name | PlanningGroup 1 |
	When I view planning periods for planning group 'PlanningGroup 1'
	And I click create next planning period
	Then I should see a planning period between '2016-07-01' and '2016-07-31'

