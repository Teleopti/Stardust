@ignore
Feature: AddActivity
	As a team leader
	I want to reassign my agents to a different task by add activity

Scenario: Should not see add activity menu item without permission
	Given I am a team leader without 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected 1 agents
	Then I could not see 'Add Absence' menu item

Scenario: Can open add activity panel without agent selected
	Given I am a team leader with 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected 0 agents
	Then I could see 'Add Absence' menu item is disabled

Scenario: Could not add activity when data is invalid
	Given I am a team leader with 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected 1 agents
	And I open 'Add Activity' panel for add activity
	Then I could see the button to apply 'Add Absence' is disabled

Scenario: Default activity start time should be latest start time of selected agents
	Given I am a team leader with 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected agent 'Ashley Andeen' with schedule start time '2016-01-01 09:00'
	And I selected agent 'Steve Novack' with schedule start time '2016-01-01 10:30'
	And I open 'Add Activity' panel for add activity
	Then I could see default start time of new activity is "10:30"
	And I could see default end time of new activity is "11:30"

Scenario: Should show notice when add activity finished sucessful
	Given I am a team leader with 'Add Activity' permission
	When I am viewing team schedule for '2016-01-01'
	And I selected agent 'Ashley Andeen' with schedule start time '2016-01-01 09:00'
	And I selected agent 'Steve Novack' with schedule start time '2016-01-01 10:30'
	And I open 'Add Activity' panel for add activity
	And I applied 'Add absence'
	Then I could see a notice that add absence finished successfully

Scenario: Should show error messages when add activity finished with error
	Given I am a team leader with 'Add Activity' permission
	When I add and impossible activity for agent 'Ashley Andeen' (How to do it?)
	Then I could see a notice that add absence finished with error
	And I can see detail of errors
