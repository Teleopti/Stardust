@OnlyRunIfEnabled('PreferenceAlertWhenMinOrMaxHoursBroken')
Feature: Preference Alert When Broken Min Or Max Hours
         In order to not break min and max hours per week when I submit preference
         As an agent
         I want to be informed if I have broken the min or max hours per week
 
Scenario: Show min and max hours per week input to preference
Given I am an agent
And I have a shift bag with start times 8 to 10 and end times 15 to 17
And I have a preference with start time limitation between 8 and 9
When I view preferences
Then I should see min hours as '36'
And I should see max hours as '63'
 
Scenario: Show alert when min hours are larger than contract setting
Given I am an agent
And I have a contract with max hours '60'
And I have a shift bag with start times 7 to 9 and end times 18 to 19
And I have a preference with start time limitation between 7 and 8
When I view preferences
Then I should be alerted for the max hours
 
Scenario: Show alert when max hours are less than workflow control setting
Given I am an agent
And I have a workflow control set with min hours '28'
And I have a shift bag with start times 12 to 14 and end times 14 to 15
And I have a preference with start time limitation between 13 and 14
When I view preferences
Then I should be alerted for the min hours
 
Scenario: Show what min and max hours per week should be
Given I am an agent
And I have a workflow control set with min hours '28'
And I have a contract with max hours '60'
When I view preferences
Then I should see what min and max hours per week should be

Scenario: Should not alert when neither min nor max hours are broken
Given I am an agent
And I have a workflow control set with min hours '28'
And I have a contract with max hours '60'
And I have a shift bag with start times 8 to 10 and end times 15 to 16
And I have a preference with start time limitation between 8 and 9
When I view preferences
Then I should not be alerted