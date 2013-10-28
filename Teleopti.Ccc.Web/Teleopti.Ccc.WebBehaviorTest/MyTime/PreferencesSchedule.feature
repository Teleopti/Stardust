@WatiN
Feature: Preferences schedule
	In order to view and submit when I prefer to work
	As an agent
	I want to see when I have been scheduled already

Scenario: See scheduled shift
	Given I am an agent
	And I have a shift today
	And My schedule is published
	When I view preferences
	Then I should see my shift

Scenario: Can not add preference on scheduled day
	Given I am an agent
	And I have a shift today
	And My schedule is published
	When I view preferences
	Then I should see my shift
	And I should not be able to add preference today

Scenario: See scheduled dayoff
	Given I am an agent
	And I have a dayoff today
	And My schedule is published
	When I view preferences
	Then I should see the dayoff

Scenario: See scheduled absence
	Given I am an agent
	And I have a shift today
	And I have a full-day absence today
	And My schedule is published
	When I view preferences
	Then I should see the absence

Scenario: See scheduled absence when no underlying shift
	Given I am an agent
	And I have a full-day absence today
	And My schedule is published
	When I view preferences
	Then I should see the absence

Scenario: See scheduled absence on schedule dayoff
	Given I am an agent
	And I have a dayoff today
	And I have a full-day absence today
	And My schedule is published
	When I view preferences
	Then I should see the absence

Scenario: See scheduled absence on contract dayoff
	Given I am an agent that has a dayoff today according to my contract
	And I have a full-day absence today
	And My schedule is published
	When I view preferences
	Then I should see the absence