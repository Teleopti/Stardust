@People
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
	And WA People exists
	| Name		|
	| Ashley	|
	| John		|
	| Pierre	|
	And Person 'Ashley' has role 'Agent'
	And Person 'Ashley' has role 'TeamLeader'
	And Person 'John' has role 'Agent'
	And Person 'Pierre' has role 'Agent'


Scenario: Show selected people in workspace
	Given I view people
	When I select person 'Ashley'
	Then I should see 'Ashley' in the workspace
	When I select person 'John'
	Then I should see 'John' in the workspace
	When I select person 'Pierre'
	Then I should see 'Pierre' in the workspace


Scenario: Remove selected people from workspace
	Given I view people
	And I have selected people
	| Name		|
	| Ashley	|
	| John		|
	| Pierre	|
	When I remove 'Ashley' from the workspace
	Then I should not see 'Ashley' in the workspace
	And I should see 'John' in the workspace
	And I should see 'Pierre' in the workspace


Scenario: Grant roles on people
	Given I view people
	And I have selected people
	| Name		|
	| Ashley	|
	| John		|
	| Pierre	|
	When I navigate to grant page
	Then The grant page is shown
	When I select the role 'TeamLeader' to grant
	And I press the save button
	Then Person 'Ashley' should have role 'TeamLeader'
	And Person 'John' should have role 'TeamLeader'
	And Person 'Pierre' should have role 'TeamLeader'


Scenario: Revoke roles on people
	Given I view people
	And I have selected people
	| Name		|
	| Ashley	|
	| John		|
	| Pierre	|
	When I navigate to revoke page
	Then The revoke page is shown
	When I select the role 'Agent' to revoke
	And I press the save button
	Then Person 'Ashley' should not have role 'Agent'
	And Person 'John' should not have role 'Agent'
	And Person 'Pierre' should not have role 'Agent'