@People
@OnlyRunIfEnabled('Wfm_PeopleWeb_PrepareForRelease_74903')
Feature: Roles
	In order to edit roles
	As a teamleader
	I want to be able to grant or revoke roles from people

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
	And there is a role with
	| Field          | Value |
	| Name           | SuperAdmin |
	| Description    | SuperAdmin |
	And WA People exists
	| Name		|
	| Ashley	|
	| Anthony	|
	| Abraham	|
	And Person 'Ashley' has role 'Agent'
	And Person 'Ashley' has role 'SuperAdmin'
	And Person 'Ashley' has role 'TeamLeader'
	And Person 'Anthony' has role 'Agent'
	And Person 'Abraham' has role 'Agent'
	And I view people
	And I searched for 'a'
	And The search list is populated


Scenario: Show selected people in workspace
	When I select person 'Ashley'
	Then I should see 'Ashley' in the workspace
	When I select person 'Anthony'
	Then I should see 'Anthony' in the workspace
	When I select person 'Abraham'
	Then I should see 'Abraham' in the workspace


Scenario: Remove selected people from workspace
	Given I have selected people
	| Name		|
	| Ashley	|
	| Anthony	|
	| Abraham	|
	When I remove 'Ashley' from the workspace
	Then I should not see 'Ashley' in the workspace
	And I should see 'Anthony' in the workspace
	And I should see 'Abraham' in the workspace


Scenario: Grant roles on people
	Given I have selected people
	| Name		|
	| Ashley	|
	| Anthony	|
	| Abraham	|
	When I navigate to grant page
	Then The grant page is shown
	When I select the role 'TeamLeader' to grant
	And I press the save button
	Then Person 'Ashley' should have role 'TeamLeader'
	And Person 'Ashley' should have role 'SuperAdmin'
	And Person 'Anthony' should have role 'TeamLeader'
	And Person 'Anthony' should not have role 'SuperAdmin'
	And Person 'Abraham' should have role 'TeamLeader'
	And Person 'Abraham' should not have role 'SuperAdmin'


Scenario: Revoke roles on people
	Given I have selected people
	| Name		|
	| Ashley	|
	| Anthony	|
	| Abraham	|
	When I navigate to revoke page
	Then The revoke page is shown
	When I select the role 'Agent' to revoke
	And I press the save button
	Then Person 'Ashley' should not have role 'Agent'
	And Person 'Anthony' should not have role 'Agent'
	And Person 'Abraham' should not have role 'Agent'