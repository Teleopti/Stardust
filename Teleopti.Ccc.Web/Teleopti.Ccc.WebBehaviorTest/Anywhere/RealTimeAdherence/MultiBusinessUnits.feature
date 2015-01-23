Feature: Real time adherence multiple business units
	In order to ...
	As a real time analyst
	I want to see ...

@OnlyRunIfEnabled('RTA_MonitorMultipleBusinessUnits_28348')
Scenario: Should display sites of a selected business unit
	Given the time is '2014-08-01 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |	
	| Access to everyone					 | True              |
	| Access to real time adherence overview | True              |
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
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value       |
	 | Team       | Green |
	 | Start Date | 2014-01-21  |
	When I view Real time adherence overview
	And I choose business unit 'Business Unit 1'
	Then I should see the site 'Paris'

@OnlyRunIfEnabled('RTA_MonitorMultipleBusinessUnits_28348')
Scenario: Should be able to see all agents state updates of a team within a specific business unit
	Given I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |	
	| Access to everyone					 | True              |
	| Access to real time adherence overview | True              |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 2 |
	And there is a site 'Paris' on business unit 'Business Unit 1'
	And there is a site 'London' on business unit 'Business Unit 2'
	And there is a team named 'Red' on site 'Paris'
	And there is an activity with
	| Field         | Value           |
	| Name          | Phone           |
	| Business Unit | Business Unit 1 |
	And there is an activity with
	| Field         | Value           |
	| Name          | Lunch           |
	| Business Unit | Business Unit 1 |
	And there is a datasouce with id 6
	And I am located in 'London'
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-01-21   |
	 | External Logon | Pierre Baldi |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2014-01-21 12:00 |
	| End time                 | 2014-01-21 13:00 |
	| Activity                 | Phone            |
	| Next activity            | Lunch            |
	| Next activity start time | 2014-01-21 13:00 |
	| Next activity end time   | 2014-01-21 13:30 |
	And there is an alarm with 
	| Field           | Value    |
	| Business Unit   | Business Unit 1    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color     | Green    |
	| Staffing effect | 0        |
	And there is an alarm with 
	| Field           | Value        |
	| Business Unit   | Business Unit 1    |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Alarm Color     | Red          |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And the time is '2014-01-21 12:30:00'
	When I view Real time adherence overview
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And I choose business unit 'Business Unit 1'
	And I click the site 'Paris'
	And I view real time adherence for team 'Red'
	And the time is '2014-01-21 12:45:00'
	Then I should see real time agent details for 'Pierre Baldi'
		| Name                     |                  |
		| Name                     | Pierre Baldi     |
		| State                    | Pause            |
		| Activity                 | Phone            |
		| Next activity            | Lunch            |
		| Next activity start time | 2014-01-21 13:00 |
		| Alarm                    | Not adhering     |
		| Alarm Time               | 0:15:00          |
		| Alarm Color              | Red              |
