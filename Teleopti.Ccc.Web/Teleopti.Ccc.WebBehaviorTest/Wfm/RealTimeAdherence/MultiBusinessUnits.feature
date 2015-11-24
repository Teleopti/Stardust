Feature: Multiple business units
	In order to ...
	As a real time analyst
	I want to see ...

Background:
	Given there is a switch

Scenario: See sites of a selected business unit
	Given the time is '2014-08-01 13:00'
	And I have a role with full access
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
	 | Team       | Red        |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Green      |
	 | Start Date | 2014-01-21 |
	When I view Real time adherence sites
	And I change to business unit 'Business Unit 1'
	Then I should see the site 'Paris'

Scenario: See all agents state updates of a team within a specific business unit
	Given I have a role with full access
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a site 'Paris' on business unit 'Business Unit 1'
	And there is a team named 'Red' on site 'Paris'
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-01-21   |
	And there is an alarm with 
	| Field         | Value           |
	| Name          | Positive        |
	| Business Unit | Business Unit 1 |
	| Phone state   | Ready           |
	And the time is '2014-01-21 12:30:00'
	When I view Real time adherence sites
	And I change to business unit 'Business Unit 1'
	And I view real time adherence for agents on team 'Red'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	Then I should see agent details for 'Pierre Baldi'
		| Name  |              |
		| Name  | Pierre Baldi |
		| State | Ready        |
