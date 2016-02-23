@ignore
Feature: Intraday
    In order to be in control of my part of the business
    As an intraday analyst
    I want to be able to monitor my part of the business

Background:
    Given I have a role with
    | Field              | Value |
    | Access to Intraday | True  |
    And There is a skill to monitor called 'Skill A'

Scenario: Create Skill Area
    Given I am viewing intraday page
    And I select to create a new Skill Area
    And I name the Skill Area 'my Area'
    And I select the skill 'Skill A'
    When I am done creating Skill Area 
	Then I select to monitor skill area 'my Area'
    And I should monitor 'my Area'

Scenario: Remove Skill Area
	Given there is a Skill Area called 'Area one' that monitors skill 'Skill A'
    And I am viewing intraday page
	When I select to monitor skill area 'Area one'
    And I select to remove 'Area one'
	Then I should no longer be able to monitor 'Area one'