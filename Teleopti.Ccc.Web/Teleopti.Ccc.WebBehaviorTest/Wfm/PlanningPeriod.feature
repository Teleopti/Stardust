Feature: Planning Period
	As a resource planner
	I want to work on planning periods

Scenario: show the next planning period
	Given the time is '2014-04-10'
	And I am swedish
	And I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to resource planner            | True              |
	When I view Resource planner
	Then I should see planning period from '2014-05-01'to '2014-05-31'

@ignore
Scenario: schedule the next planning period
	Given the time is '2014-04-10'
	And I have a role with
		| Field									| Value            |
        | Name									| Resource Planner |
		| Access to resource planner			| True             | 
	When I view Resource planner
	And I open planning period
	And  I click schedule
	Then I should see '0'

@Ignore
Scenario: Publish the schedules for next planning period
	Given the time is '2014-04-10'
	And I have a role with
		| Field									| Value            |
        | Name									| Resource Planner |
		| Access to resource planner			| True             | 
	When I view Resource planner
	And I open planning period
	And  I click schedule
	And I click publish
	Then I should see the planning period as published in the summary

@OnlyRunIfEnabled('Wfm_ChangePlanningPeriod_33043')
Scenario: Update the next planning period
	Given the time is '2014-04-10' 
	And I am swedish
	And I have a role with
		| Field									| Value            |
        | Name									| Resource Planner |
		| Access to resource planner			| True             | 
	And there are shift categories
	| Name  |
	| Day   |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2014-03-30         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2014-04-06 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2014-04-06 |
	And GroupingReadModel is updated
	When I view Resource planner
	And I open planning period
	And I click next planning period
	And I update planning period to two week 
	Then I should see updated period from '2014-04-13'to '2014-04-26'

Scenario: Create the next planning period
	Given the time is '2014-04-10'
	And I am swedish
	And I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to resource planner            | True              |
	When I view Resource planner
	And I create new planning period
	And I view Resource planner
	Then I should see planning period from '2014-06-01'to '2014-06-30'