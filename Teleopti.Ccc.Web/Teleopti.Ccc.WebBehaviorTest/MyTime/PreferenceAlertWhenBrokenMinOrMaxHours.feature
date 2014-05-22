@OnlyRunIfEnabled('PreferenceAlertWhenMinOrMaxHoursBroken')
Feature: Preference Alert When Broken Min Or Max Hours
         In order to not break min and max hours per week when I submit preference
         As an agent
         I want to be informed if I have broken the min or max hours per week

@ignore 
Scenario: Show min and max hours per week that the entered preference could result in during scheduling
Given I am an agent
And I have a shift bag with start times 8 to 9 and end times 12 to 20
And I have a preference with work time limitation between 5 and 9
When I view preferences
Then I should see min hours per week as '23'
And I should see max hours per week as '81'
 
@ignore
Scenario: Show alert when current min hours per week is larger than max hours per week on contract
Given I am an agent
And I have a contract with max hours '60'
When I view preferences
And I insert preferences for a week with work time '9' to '12' hours per day
Then I should be alerted for the max hours
 
@ignore
Scenario: Show alert when current max hours per week is less than min hours per week on WCS
Given I am an agent
And I have a workflow control set with min hours '28'
When I view preferences
And I insert preferences for a week with work time '1' to '3' hours per day
Then I should be alerted for the min hours

@ignore
Scenario: Should not alert when neither min nor max hours are broken
Given I am an agent
And I have a workflow control set with min hours '28'
And I have a contract with max hours '60'
When I view preferences
And I insert preferences for a week with work time '6' to '10' hours per day
Then I should not be alerted