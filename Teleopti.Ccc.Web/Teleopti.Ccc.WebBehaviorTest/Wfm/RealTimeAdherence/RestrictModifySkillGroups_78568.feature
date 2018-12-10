@RTA
@OnlyRunIfEnabled('RTA_RestrictModifySkillGroups_78568')
Feature: Restrict modify skill groups
  In order to prevent users to modifying skill groups
  As a system administrator
  I don't want the modify skill group button to be displayed for users

  Scenario: Modify skill groups restricted by access permission in agents view
	Given I have a role with
	  | Field                        | Value |
	  | Access to modify skill group | False |
	When I monitor agents adherence
	Then I should not be able to modify skill groups

  Scenario: Modify skill groups granted by access permission in agents view
	Given I have a role with
	  | Field                        | Value |
	  | Access to modify skill group | True  |
	When I monitor agents adherence
	Then I should be able to modify skill groups

  Scenario: Modify skill groups restricted by access permission in overview
	Given I have a role with
	  | Field                        | Value |
	  | Access to modify skill group | False |
	When I monitor organization adherence
	Then I should not be able to modify skill groups

  Scenario: Modify skill groups granted by access permission in overview
	Given I have a role with
	  | Field                        | Value |
	  | Access to modify skill group | True  |
	When I monitor organization adherence
	Then I should be able to modify skill groups
