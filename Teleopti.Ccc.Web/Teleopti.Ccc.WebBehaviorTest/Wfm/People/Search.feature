@People
@OnlyRunIfEnabled('Wfm_PeopleWeb_PrepareForRelease_47766')
Feature: Search
	In order to find a person
	As a teamleader
	I want to be able to seach and filter for people

Background:
	Given I have a role with full access
	And there is a role with
	| Field          | Value |
	| Name           | Agent |
	| Description    | Agent |
	And there is a role with
	| Field          | Value |
	| Name           | TeamLeader |
	| Description    | TeamLeader |
	And WA People exists
	| Name		|
	| Ashley	|
	| John		|
	| Pierre	|
	And Person 'Ashley' has role 'Agent'
	And Person 'Ashley' has role 'TeamLeader'
	And Person 'John' has role 'Agent'
	And Person 'Pierre' has role 'Agent'

@Ignore
Scenario: Search for a selection of users
	Given I view people
	When Searching for 'London'
	Then I should see all matches for 'London'
	And I should see columns
	| Columns	|
	| Ashley	|
	| John		|
	| Pierre	|



