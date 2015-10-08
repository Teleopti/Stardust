@ignore
Feature: Real time adherence multiple business units
	In order to ...
	As a real time analyst
	I want to see ...

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

Scenario: Should be able to see all agents state updates of a team within a specific business unit
	Given I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |	
	| Access to everyone					 | True              |
	| Access to real time adherence overview | True              |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a site 'Paris' on business unit 'Business Unit 1'
	And there is a team named 'Red' on site 'Paris'
	And there is a datasouce with id 6
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-01-21   |
	 | External Logon | Pierre Baldi |
	And there is an alarm with 
	| Field         | Value           |
	| Name          | Positive        |
	| Business Unit | Business Unit 1 |
	| Phone state   | Ready           |
	And the time is '2014-01-21 12:30:00'
	When I view Real time adherence overview
	And I choose business unit 'Business Unit 1'
	And I view real time adherence for team 'Red'
	And 'Pierre Baldi' sets his phone state to 'Ready' on datasource 6
	Then I should see real time agent details for 'Pierre Baldi'
		| Name                     |                  |
		| Name                     | Pierre Baldi     |
		| State                    | Ready            |
