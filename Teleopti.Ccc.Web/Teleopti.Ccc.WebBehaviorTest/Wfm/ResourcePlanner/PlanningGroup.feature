@WFM
Feature: PlanningGroup
	As a resource planner 
	I need to generate a plan (schedule&opt) for my part of the organization,
		so that Stockholm can be planned by 4-weeks and Paris by monthly periods,
		so that I can plan for Customer Support and Lisa can plan for Sales,
		so that John can plan for Invoice in London,
		so that Fixed agents in London have exactly 2 DOs off per week.

Scenario: Create planning group
	Given there is a site named 'Site 1'
	And there is a site named 'Site 2'
	And there is an activity named 'Phone'
	And there is a skill named 'Skill 1' with activity 'Phone'
	And there is a team named 'Team 1' on 'Site 1'
	And there is a team named 'Team 2' on 'Site 2'
	And I have a role with
	| Field                     | Value            |
	| Name                      | Resource Planner |
	| Access to team            | Team 1, Team 2   |
	| Access to resourceplanner | true             |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Team 1     |
	 | Start Date | 2015-01-21 |
	 | Skill      | Skill 1    |
	And John King has a person period with
	 | Field      | Value      |
	 | Team       | Team 1     |
	 | Start Date | 2015-01-21 |
	When I am viewing create planning group page
	And I input planning group name 'PlanningGroup 1'
	And I select the team 
	 | Team   | Site   |
	 | Team 1 | Site 1 |
	 | Team 2 | Site 2 |
	 And I select the skill 
	 | Skill   |
	 | Skill 1 |
	And I save planning group
	Then I should see 'PlanningGroup 1' in the planning group list

@ignore
Scenario: Delete planning group
	Given there is a site named 'Site 1'
	And there is a team named 'Team 1' on 'Site 1'
	And I have a role with
	| Field                     | Value            |
	| Name                      | Resource Planner |
	| Access to team            | Team 1           |
	| Access to resourceplanner | true             |
	And there is an planning group with
	| Field               | Value           |
	| Planning group name | PlanningGroup 1 |
	| Team                | Team 1          |
	When I am viewing planning group list page
	And I click edit planning group 'PlanningGroup 1'
	And I click delete planning group 
	And I confirm deletion
	Then I should not see 'PlanningGroup 1' in the planning group list

Scenario: Edit planning group
	Given there is a site named 'Site 1'
	And there is a site named 'Site 2'
	And there is a team named 'Team 1' on 'Site 1'
	And there is a team named 'Team 2' on 'Site 2'
	And I have a role with
	| Field                     | Value            |
	| Name                      | Resource Planner |
	| Access to team            | Team 1, Team 2   |
	| Access to resourceplanner | true             |
	And there is an planning group with
	| Field               | Value           |
	| Planning group name | PlanningGroup 1 |
	| Team                | Team 1          |
	When I am viewing planning group list page
	And I click edit planning group 'PlanningGroup 1'
	And I input planning group name 'PlanningGroup 2'
	And I select the team 
	 | Team   | Site   |
	 | Team 2 | Site 2 |
	And I save planning group
	Then I should see 'PlanningGroup 2' in the planning group list
