@People
Feature: Roles
	In order to edit roles
	As a teamleader
	I want to be able to grant or revoke roles from people

Background:
	Given I have a role with
	| Field              | Value |
	| Access to People | True  |
	And I have a role with full access
	And there is a role with
	 | Field          | Value |
	 | Name           | Agent |
	 And there is a role with
	 | Field          | Value |
	 | Name           | TeamLeader |
	And Person 'Ashley' exists
	And Person 'John' exists
	And Person 'Pierre' exists

@ignore
Scenario: Show selected people in workspace
	Given I view people
	When I select person 'Ashley'
	And I select person 'John'
	And I select person 'Pierre'
	Then I should see 'Ashley' in the workspace
	And I should see 'John' in the workspace
	And I should see 'Pierre' in the workspace

@ignore
Scenario: Remove selected people from workspace
	Given Person 'Ashley' is selected
	And Person 'John' is selected
	And Person 'Pierre' is selected
	When I remove 'Ashley' from the workspace
	Then I should not see 'Ashley' in the workspace
	And I should see 'John' in the workspace
	And I should see 'Pierre' in the workspace

@ignore
Scenario: Grant roles on people
	Given Person 'Ashley' is selected
	And Person 'John' is selected
	And Person 'Pierre' is selected
	And All of them has role 'Agent'
	And Person 'Ashley' has role 'TeamLeader'
	When I press the grant button
	Then The grant page is shown
	Given The grant page is shown
	When I select the role 'TeamLeader'
	And I press the save button
	Then Person 'Ashley' should have role 'TeamLeader'
	And Person 'John' should have role 'TeamLeader'
	And Person 'Pierre' should have role 'TeamLeader'

@ignore
Scenario: Revoke roles on people
	Given Person 'Ashley' is selected
	And Person 'John' is selected
	And Person 'Pierre' is selected
	And All of them has role 'Agent'
	And Person 'Ashley' has role 'TeamLeader'
	When I press the revoke button
	Then The revoke page is shown
	Given The revoke page is shown
	When I select the role 'Agent'
	And I press the save button
	Then Person 'Ashley' should not have role 'Agent'
	And Person 'John' should not have role 'Agent'
	And Person 'Pierre' should not have role 'Agent'