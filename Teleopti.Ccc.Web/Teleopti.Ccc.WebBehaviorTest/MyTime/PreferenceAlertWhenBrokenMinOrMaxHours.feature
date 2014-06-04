@OnlyRunIfEnabled('Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635')
Feature: Preference Alert When Broken Min Or Max Hours
         In order to not break min and max hours per week when I submit preference
         As an agent
         I want to be informed if I have broken the min or max hours per week

Scenario: Show min and max hours per week that the entered preference could result in during scheduling
Given I am an agent
And I have a shift bag with start times 8 to 9 and end times 12 to 20
And I have a preference with work time limitation between 5 and 9
When I view preferences
Then I should see min hours per week as '23'
And I should see max hours per week as '81'

Scenario: Do not show min and max hours per week for week that is not in current period and without published schedule
Given I am an agent
And I have a shift bag with start times 8 to 9 and end times 12 to 20
And I have a preference with work time limitation between 5 and 9
When I view preferences
Then I should not see min and max hours per week for one week before

Scenario: Show alert when current min hours per week is larger than max hours per week on contract
Given I am an agent
And I have a contract with:
         | Field              | Value |
         | max hours per week | 60    |
And I have a shift bag with start times 8 to 9 and end times 18 to 20
And I have a preference with work time limitation between 7 and 9
When I view preferences
Then I should be alerted for the max hours

Scenario: Show alert when current max hours per week is less than min hours per week on WCS
Given I am an agent
And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	| Min time per week          | 60                 |
And  I have a shift bag with start times 9 to 10 and end times 15 to 16
And I have a preference with work time limitation between 7 and 9
When I view preferences
Then I should be alerted for the min hours

Scenario: Should not alert when neither min nor max hours are broken
Given I am an agent
And I have a contract with:
         | Field              | Value |
         | max hours per week | 80    |
And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	| Min time per week          | 60                 |
And  I have a shift bag with start times 8 to 10 and end times 15 to 20
And I have a preference with work time limitation between 7 and 10
When I view preferences
Then I should not be alerted