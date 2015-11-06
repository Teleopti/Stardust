
Feature: Real time adherence change schedule
	In order to ...
	As a real time analyst
	I want to see ...

Scenario: Should be able to change schedule from agent state overview
	Given  the time is '2014-09-09 12:30:00'
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	| Field                                  | Value       |
	| Name                                   | Team leader |
	| Access to team                         | Red         |
	| Access to real time adherence overview | True        |
	| Access to Anywhere                     | true        |
	And there is a workflow control set with
	| Field                      | Value                      |
	| Name                       | Schedule published to 0909 |
	| Schedule published to date | 2014-09-09                 |
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Red        |
	| Start date | 2014-01-01 |
	And 'Pierre Baldi' has the workflow control set 'Schedule published to 0909'
	And there are shift categories
	| Name  |
	| Day   |
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2014-09-09 08:00 |
	| End time       | 2014-09-09 17:00 |
	When I view real time adherence for agents on team 'Red'
	And the time is '2014-09-09 12:45:00'
	And I click agent state of 'Pierre Baldi'
	Then I can 'change schedule' in agent menu
