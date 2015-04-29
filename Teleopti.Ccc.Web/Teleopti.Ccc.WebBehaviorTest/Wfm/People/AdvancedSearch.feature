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

Scenario: match all search terms by default
	When I view people
	And I search people with keyword 'Team1 Ashley'
	Then I should see 'Ashley Andeen' in people list
	And I should not see 'Ashley Smith' in people list

@ignore
Scenario: match any search term
	When I view people
	And I search people with keyword 'Team1 Smith' matching any keywords
	Then I should see 'Ashley Andeen' in people list
	And I should see 'Ashley Smith' in people list

@ignore
Scenario: match any search terms in a field
	When I view people
	And I search 'Andeen Smith' in 'last name' field
	Then I should see 'Ashley Andeen' in people list
	And I should see 'Ashley Smith' in people list

@ignore
Scenario: match all search terms in different fields
	When I view people
	And I search with
	| Field        | Value        |
	| last name    | Andeen Smith |
	| organization | Team1        |
	Then I should see 'Ashley Andeen' in people list

@ignore
Scenario: match entire quoted search term
	When I view people
	And I search people with keyword '"Team1 Smith"'
	Then I should see no result

@ignore
Scenario: match entire quoted search term in different fields
	When I view people
	And I search with
	| Field        | Value          |
	| last name    | "Andeen Smith" |
	| organization | Team1          |
	Then I should see no result