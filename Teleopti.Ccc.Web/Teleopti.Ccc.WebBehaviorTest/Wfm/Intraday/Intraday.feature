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
	 Then I should see that Skill Area 'my Area' is selectable
	 And I select to view details for skill area 'my Area'
    And I should monitor 'my Area'

Scenario: Remove Skill Area
    Given I am viewing intraday page
	And There is a skill to monitor called 'Skill B'
	And there is a Skill Area called 'my Area' that monitors skill 'Skill B'
	And I select to monitor 'my Area'
    When I select to remove 'my Area'
	Then I should no longer be able to monitor 'my Area'
	And I should see that the skill 'Skill A' is selected in monitor view