@People
@OnlyRunIfEnabled('Wfm_PeopleWeb_PrepareForRelease_74903')
Feature: Search
	In order to find a person
	As a teamleader
	I want to be able to seach and filter for people

Background:
	Given I have a role with full access
	And there is a business unit named 'OtherBU'
	And there is a site 'OtherSite' on business unit 'OtherBU'
	And there is a team named 'OtherTeam' on site 'OtherSite'
	And WA Person 'OtherPerson' exists on team 'OtherTeam'
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
	Then I should see '2' or more matches for 'le'


Scenario: Search on different business unit
	When I pick business unit 'OtherBU'
	And Searching for 'OtherPerson'
	Then I should see '1' matches for 'OtherPerson'
	When Searching for 'Ashley'
	Then I should see '0' matches for 'Ashley'
