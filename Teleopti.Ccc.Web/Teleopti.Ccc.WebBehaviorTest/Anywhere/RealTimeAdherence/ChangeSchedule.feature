Feature: Real time adherence change schedule
	In order to ...
	As a real time analyst
	I want to see ...

Background:
	Given there is a switch

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
	When I view real time adherence view for team 'Red'
	And the time is '2014-09-09 12:45:00'
	And I click agent state of 'Pierre Baldi'
	And I click 'change schedule' in agent menu
	Then I should see schedule for 'Pierre Baldi'
	And I should see schedule menu

Scenario: Should be able to change schedule for multiple business units
	Given the time is '2014-09-09 12:30:00'
	And I have a role with
	| Field              | Value             |
	| Name               | Real time analyst |
	| Access to everyone | True              |
	| Access to Anywhere | true              |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a scenario
	| Field         | Value           |
	| Name          | Scenario 1      |
	| Business Unit | Business Unit 1 |
	And there is a site 'Paris' on business unit 'Business Unit 1'
	And there is a team named 'Red' on site 'Paris'
	And there is an activity with
	| Field         | Value           |
	| Name          | Phone           |
	| Color         | Green           |
	| Business Unit | Business Unit 1 |
	And there is an activity with
	| Field         | Value           |
	| Name          | Lunch           |
	| Color         | Yellow          |
	| Business Unit | Business Unit 1 |
	And there is a shift category with
	| Field         | Value           |
	| Name          | Day             |
	| Business Unit | Business Unit 1 |
	And there is a workflow control set with
	| Field                      | Value                      |
	| Name                       | Schedule published to 0909 |
	| Schedule published to date | 2014-09-09                 |
	| Business Unit              | Business Unit 1            |
	And 'Pierre Baldi' has the workflow control set 'Schedule published to 0909'
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-09-09   |
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Scenario       | Scenario 1       |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2014-09-09 08:00 |
	| End time       | 2014-09-09 17:00 |
	When I view schedules for 'Red' on '2014-09-09'
	And I choose business unit 'Business Unit 1'
	And I click person name 'Pierre Baldi'
	And I choose to 'add activity' from schedule menu
	And I input these add activity values
	| Field      | Value |
	| Activity   | Lunch |
	| Start time | 12:00 |
	| End time   | 13:00 |
	And I initiate 'apply'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Field      | Value  |
	| Start time | 12:00  |
	| End time   | 13:00  |
	| Color      | Yellow |
