Feature: Preferences feedback
	In order to know at which times I might work
	As an agent
	I want feedback for my preferences

Scenario: Feedback for a day with shift category preference
	Given I am an agent
	And I am american
	And I have a shift bag with two categories with shift from 8 to 16 and from 12 to 19
	And I have preference for the first category for '2014-05-03'
	When I view preferences for date '2014-05-02'
	Then I should see the start time boundry 8 to 8
	And I should see the end time boundry 16 to 16
	And I should see the contract time boundry 8 to 8

Scenario: Feedback for a day with start time limitation preference
	Given I am an agent
	And I have a shift bag with start times 8 to 13 and end times 12 to 22
	And I have a preference with start time limitation between 8 and 10 for '2014-05-03'
	When I view preferences for date '2014-05-02'
	Then I should see the start time boundry 8 to 10

Scenario: Feedback for a day with end time limitation preference
	Given I am an agent
	And I have a shift bag with start times 8 to 9 and end times 12 to 22
	And I have a preference with end time limitation between 13 and 19 for '2014-05-03'
	When I view preferences for date '2014-05-02'
	Then I should see the end time boundry 13 to 19

Scenario: Feedback for a day with work time limitation preference
	Given I am an agent
	And I have a shift bag with start times 8 to 9 and end times 12 to 22
	And I have a preference with work time limitation between 4 and 5 for '2014-05-03'
	When I view preferences for date '2014-05-02'
	Then I should see the contract time boundry 4 to 5

Scenario: Feedback for a day with lunch start time limitation preference
	Given I am an agent
	And there is an activity named 'Lunch'
	And I have a shift bag with one shift 8 to 17 and Lunch 12 to 13 and one shift 9 to 19 and Lunch 13 to 14
	And I have a preference with Lunch start time limitation between 13 and 13 for '2014-05-03'
	When I view preferences for date '2014-05-02'
	Then I should see the start time boundry 9 to 9
	And I should see the end time boundry 19 to 19
	And I should see the contract time boundry 10 to 10 

Scenario: Feedback for a day with lunch end time limitation preference
	Given I am an agent
	And there is an activity named 'Lunch'
	And I have a shift bag with one shift 9 to 18 and Lunch 12 to 13 and one shift 9 to 19 and Lunch 12 to 14
	And I have a preference with Lunch end time limitation between 12 and 13 for '2014-05-03'
	When I view preferences for date '2014-05-02'
	Then I should see the start time boundry 9 to 9
	And I should see the end time boundry 18 to 18
	And I should see the contract time boundry 9 to 9 

Scenario: Feedback for a day with lunch length limitation preference
	Given I am an agent
	And there is an activity named 'Lunch'
	And I have a shift bag with one shift 8 to 17 and Lunch 12 to 13 and one shift 9 to 19 and Lunch 12 to 14
	And I have a preference with Lunch length limitation of 1 hour for '2014-05-03'
	When I view preferences for date '2014-05-02'
	Then I should see the start time boundry 8 to 8
	And I should see the end time boundry 17 to 17
	And I should see the contract time boundry 9 to 9 

Scenario: Feedback for a day with start time limitation availability
	Given I am an agent
	And I have a shift bag with start times 8 to 13 and end times 12 to 22
	And I have a availabilty with earliest start time at 10
	When I view preferences for date '2014-05-02'
	Then I should see the start time boundry 10 to 13

Scenario: Feedback for a day with end time limitation availability
	Given I am an agent
	And I have a shift bag with start times 8 to 13 and end times 12 to 22
	And I have a availabilty with latest end time at 21
	When I view preferences for date '2014-05-02'
	Then I should see the end time boundry 12 to 21

Scenario: Feedback for a day with work time limitation availability
	Given I am an agent
	And I have a shift bag with start times 8 to 13 and end times 12 to 22
	And I have a availabilty with work time between 5 and 7 hours
	When I view preferences for date '2014-05-02'
	Then I should see the contract time boundry 5 to 7

Scenario: Feedback for a day with availability and preference
	Given I am an agent
	And I have a shift bag with two categories with shift start from 8 to 10 and from 12 to 14 and end from 16 to 18 and from 12 to 20
	And I have preference for the first category for '2014-05-03'
	And I have a availabilty with earliest start time at 9
	When I view preferences for date '2014-05-02'
	Then I should see the start time boundry 9 to 10
	And I should see the end time boundry 16 to 18
	And I should see the contract time boundry 6 to 9

Scenario: Feedback for a day with a schedule, preference and availability
	Given I am an agent
	And I have a shift bag
	And I have an assigned shift with
	| Field      | Value      |
	| Date | 2014-05-02 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-05-02 |
	And I have a availabilty with earliest start time at 9
	When I view preferences for date '2014-05-02'
	Then I should see my shift for '2014-05-02'

Scenario: Feedback from conflicting preferences and availability
	Given I am an agent
	And I have a shift bag
	And I have a conflicting preference and availability for '2014-05-03'
	When I view preferences for date '2014-05-02'
	Then I should see that there are no available shifts

Scenario: Feedback from an added preference
	Given I have a role with
         | Field                          | Value |
         | Access To Extended Preferences | False |
	And I have schedule and person period
	And there is a shift category named 'Day'
	And the current time is '2014-05-02 08:00'
	And I have an open workflow control set with an allowed standard preference open from '2014-05-03' to '2014-05-05'
	And I have a shift bag
	And I am viewing preferences
	When I select an editable day without preference
	And I select shift category 'Day' as standard preference
	Then I should see the preference feedback
	
Scenario: Feedback from a deleted preference
	Given I am an agent
	And the current time is '2014-05-02 08:00'
	And I have an open workflow control set with an allowed standard preference open from '2014-05-03' to '2014-05-05'
	And I have a shift bag
	And I have existing absence preference with
	| Field | Value      |
	| Date  | 2014-05-03 |
	And I am viewing preferences
	When I select an editable day with standard preference
	And I click the delete preference button
	Then I should see the preference feedback

Scenario: Feedback for a day with a shift
	Given I am an agent
	And I have a shift bag
	And I have an assigned shift with
	| Field     | Value      |
	| Date      | 2014-05-02 |
	| StartTime | 9:00       |
	| EndTime   | 17:00      |
	When I view preferences for date '2014-05-02'
	Then I should see the contract time of '8' hours

Scenario: Feedback for a day with a contract time absence
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a scheduled shift of 8 hours on '2014-04-28'
	And I have a scheduled shift of 8 hours on '2014-04-29'
	And I have a scheduled shift of 8 hours on '2014-04-30'
	And I have a scheduled shift of 8 hours on '2014-05-01'
	And I have a scheduled shift of 8 hours on '2014-05-02'
	And I have a scheduled day off on '2014-05-03'
	And I have a scheduled day off on '2014-05-04'
	And I have a full-day contract time absence on '2014-05-02'
	When I view preferences for date '2014-05-02'
	Then I should see the absence contract time of '8' hours
	And I should see a message that my preferences can result in 40 hours
