@OnlyRunIfEnabled('WfmPeople_AdvancedSearch_32973')
Feature: AdvancedSearch
	In order to find the exact group of people I want
	As a team leader
	I want to find people with advanced options

	Background: 
	Given there is a site named 'London'
	And there is a team named 'Team Red' on site 'London'
	And there is a team named 'Team Blue' on site 'London'
	And I have a role with
	 | Field              | Value       |
	 | Name               | Team leader |
	 | Access to everyone | true        |
	 | Access to people   | true        |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Team Red   |
	 | Start Date | 2015-01-21 |
	And Ashley Smith has a person period with
	 | Field      | Value      |
	 | Team       | Team Blue      |
	 | Start Date | 2015-01-21 |
	And I have a person period with
	 | Field      | Value      |
	 | Team       | Team Red      |
	 | Start Date | 2015-01-21 |

Scenario: match all search terms by default
	When I view people
	And I search people with keyword 'Red Ashley'
	Then I should see 'Andeen' in people list
	And I should not see 'Smith' in people list

@ignore
#not implemented
Scenario: match any search term
	When I view people
	And I search people with keyword 'Red Smith' matching any keywords
	Then I should see 'Andeen' in people list
	And I should see 'Smith' in people list

Scenario: match any search terms in a field
	When I view people
	And I search with
	| Field        | Value        |
	| last name    | Andeen Smith |
	Then I should see 'Andeen' in people list
	And I should see 'Smith' in people list

Scenario: match all search terms in different fields
	When I view people
	And I search with
	| Field        | Value        |
	| last name    | Andeen Smith |
	| organization | Red        |
	Then I should see 'Andeen' in people list
@ignore
Scenario: match entire quoted search term
	When I view people
	And I search people with keyword '"Team Blue" Ashley'
	Then I should see 'Smith' in people list
	And I should not see 'Andeen' in people list

Scenario: match entire quoted search term in different fields
	When I view people
	And I search with
	| Field        | Value      |
	| last name    | Andeen     |
	| organization | "Team Red" |
	Then I should see 'Andeen' in people list