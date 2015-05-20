﻿Feature: ShowBasicInformation
	In order to have an overview about the group
	As a team leader
	I want to view the basic information of all the people in specific group

Background: 
	Given there is a site named 'London'
	And there is a team named 'Team1' on site 'London'
	And there is a team named 'Team2' on site 'London'
	And I have a role with
	 | Field              | Value       |
	 | Name               | Team leader |
	 | Access to everyone | true        |
	 | Access to people   | true        |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Team1      |
	 | Start Date | 2015-01-21 |
	And John Smith has a person period with
	 | Field      | Value      |
	 | Team       | Team2      |
	 | Start Date | 2015-01-21 |
	And I have a person period with
	 | Field      | Value      |
	 | Team       | Team2      |
	 | Start Date | 2015-01-21 |
@Ignore
Scenario: Show my team members by default
	When I view people
	Then I should see 'John Smith' in people list
	And I should not see 'Ashley Andeen' in people list
@Ignore
Scenario: Should search people by keyword
	When I view people
	Then I should see 'John Smith' in people list
	And I should not see 'Ashley Andeen' in people list
	When I search people with keyword 'Team1'
	Then I should see 'Ashley Andeen' in people list
	And I should not see 'John Smith' in people list