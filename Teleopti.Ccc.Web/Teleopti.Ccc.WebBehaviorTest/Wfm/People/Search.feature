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
	| Pierre	|
	| Urban     |
	And Person 'Ashley' has role 'Agent'
	And Person 'Pierre' has role 'TeamLeader'
	And Person 'Urban' has role 'Agent'
	And I view people


Scenario: Search for a selection of users
	When Searching for 'le'
	Then I should see all matches for 'le'
	And I should see columns
	| Columns		|
	| First name	|
	| Last name		|
	| Roles			|
	| Site			|
	| Team			|



