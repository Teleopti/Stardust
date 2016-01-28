@ignore
Feature: Intraday
    In order to be in control of my part of the business
    As an intraday analyst
    I want to be able to monitor my part of the business

Background:
    Given I have a role with
    | Field                  | Value |
    | ModifySkillPack | True  |
    And There is a skill called 'Skill A'

Scenario: Create Skill Pack
    Given I am viewing intraday page
    And I select to create a new Skill Pack
    And I name the Skill Pack 'six pack'
    And I select the skill 'Skill A'
    When I am done creating Skill Pack 
	Then I should see that Skill Pack 'six pack' is selected in monitor view
    And I should see details for 'Skill A'

Scenario: Remove Skill Pack
    Given I am viewing intraday page
	And there is a Skill Pack called 'my pack'
	And I select to monitor 'my pack'
    When I select to remove 'my pack'
	Then I should no longer be able to monitor 'my pack'
	And I should see that the skill 'Skill A' is selected in monitor view