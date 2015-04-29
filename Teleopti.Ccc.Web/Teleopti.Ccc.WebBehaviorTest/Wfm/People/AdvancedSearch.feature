Feature: AdvancedSearch
	In order to find the exact group of people I want
	As a team leader
	I want to find people with advanced options

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
	And Ashley Smith has a person period with
	 | Field      | Value      |
	 | Team       | Team2      |
	 | Start Date | 2015-01-21 |
	And I have a person period with
	 | Field      | Value      |
	 | Team       | Team1      |
	 | Start Date | 2015-01-21 |

@ignore
Scenario: match all keywords when searching with mutiple keywords by default
	When I view people
	And I search people with keyword 'Team1 Asheley'
	Then I should see 'Ashley Andeen' in people list
	And I should not see agent with firstname 'Ashley' and lastname 'Andeen' in people list

@ignore
Scenario: match any keywords when searching with mutiple keywords
	When I view people
	And I set search option to match any keyword
	And I search people with keyword 'Team1 Asheley'
	Then I should see agent with firstname 'Ashley' and lastname 'Andeen' in people list
	And I should see agent with firstname 'Ashley' and lastname 'Andeen' in people list
