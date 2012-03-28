Feature: Preferences feedback
	In order to know at which times I might work
	As an agent
	I want feedback for my preferences

Scenario: Feedback for a day without restrictions
	Given I am an agent
	And I have a shift bag with start times 8 to 9 and end times 16 to 17
	When I view preferences
	Then I should see the start time boundry 8 to 9
	And I should see the end time boundry 16 to 17
	And I should see the contract time boundry 7 to 9  




Scenario: Feedback for a day with day off preference
	Given I am an agent
	And I have a shift bag
	And I have a day off preference
	When I view preferences
	Then I should see no feedback

Scenario: Feedback for a day with absence preference
	Given I am an agent
	And I have a shift bag
	And I have a absence preference
	When I view preferences
	Then I should see no feedback

Scenario: Feedback for a day with shift category preference
	Given I am an agent
	And I have a shift bag with two categories with shift from 8 to 16 and from 12 to 19
	And I have preference for the first category today
	When I view preferences
	Then I should see the start time boundry 8 to 8
	And I should see the end time boundry 16 to 16
	And I should see the contract time boundry 8 to 8

Scenario: Feedback for a day with start time limitation preference
	Given I am an agent
	And I have a shift bag with start times 8 to 13 and end times 12 to 22
	And I have a preference with start time limitation between 8 and 10
	When I view preferences
	Then I should see the start time boundry 8 to 10

Scenario: Feedback for a day with end time limitation preference
	Given I am an agent
	And I have a shift bag with start times 8 to 9 and end times 12 to 22
	And I have a preference with end time limitation between 13 and 19
	When I view preferences
	Then I should see the end time boundry 13 to 19

Scenario: Feedback for a day with work time limitation preference
	Given I am an agent
	And I have a shift bag with start times 8 to 9 and end times 12 to 22
	And I have a preference with work time limitation between 4 and 5
	When I view preferences
	Then I should see the contract time boundry 4 to 5

Scenario: Feedback for a day with lunch start time limitation preference
	Given I am an agent
	And I have a shift bag with one shift 8 to 17 and lunch 12 to 13 and one shift 9 to 19 and lunch 13 to 14
	And I have a preference with lunch start time limitation between 13 and 13
	When I view preferences
	Then I should see the start time boundry 9 to 9
	And I should see the end time boundry 19 to 19
	And I should see the contract time boundry 10 to 10 

Scenario: Feedback for a day with lunch end time limitation preference
	Given I am an agent
	And I have a shift bag with one shift 9 to 18 and lunch 12 to 13 and one shift 9 to 19 and lunch 12 to 14
	And I have a preference with lunch end time limitation between 12 and 13
	When I view preferences
	Then I should see the start time boundry 9 to 9
	And I should see the end time boundry 18 to 18
	And I should see the contract time boundry 9 to 9 

Scenario: Feedback for a day with lunch length limitation preference
	Given I am an agent
	And I have a shift bag with one shift 8 to 17 and lunch 12 to 13 and one shift 9 to 19 and lunch 12 to 14
	And I have a preference with lunch length limitation of 1 hour today
	When I view preferences
	Then I should see the start time boundry 8 to 8
	And I should see the end time boundry 17 to 17
	And I should see the contract time boundry 9 to 9 




Scenario: Feedback for a day with availability
	Given I am an agent
	And I have a shift bag
	And I have availability
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts that match the availability
	And I should see the end time boundry for the shift bag's shifts that match the availability
	And I should see the minimum contract time for the shift bag's shifts that match the availability
	And I should see the maximum contract time for the shift bag's shifts that match the availability

Scenario: Feedback for a day with start time limitation availability
	Given I am an agent
	And I have a shift bag
	And I have a availability with start time limitation of 7:00 at the earliest
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts matching the preference
	And I should see the end time boundry for the shift bag's shifts matching the preference
	And I should see the minimum contract time for the shift bag's shifts matching the preference
	And I should see the maximum contract time for the shift bag's shifts matching the preference

Scenario: Feedback for a day with end time limitation availability
	Given I am an agent
	And I have a shift bag
	And I have a availability with end time limitation of 20:00 at the latest
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts matching the preference
	And I should see the end time boundry for the shift bag's shifts matching the preference
	And I should see the minimum contract time for the shift bag's shifts matching the preference
	And I should see the maximum contract time for the shift bag's shifts matching the preference

Scenario: Feedback for a day with work time limitation availability
	Given I am an agent
	And I have a shift bag
	And I have a availability with work time limitation between 7 and 9 hours
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts matching the preference
	And I should see the end time boundry for the shift bag's shifts matching the preference
	And I should see the minimum contract time for the shift bag's shifts matching the preference
	And I should see the maximum contract time for the shift bag's shifts matching the preference



Scenario: Feedback for a day with availability and preference
	Given I am an agent
	And I have a shift bag
	And I have availability
	And I have preference with shift category AM
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts of category AM and that match the availability
	And I should see the end time boundry for the shift bag's shifts of category AM and that match the availability
	And I should see the minimum contract time for the shift bag's shifts of category AM and that match the availability
	And I should see the maximum contract time for the shift bag's shifts of category AM and that match the availability



Scenario: Feedback for a day with a schedule
	Given I am an agent
	And I have a shift
	When I view preferences
	Then I should see the start time of the shift
	And I should see the end time of the shift
	And I should see the contract time of the shift

Scenario: Feedback for a day with a schedule, preference and availability
	Given I am an agent
	And I have a shift
	And I have a shift bag
	And I have preference for shift category AM
	And I have availability
	When I view preferences
	Then I should see the start time of the shift
	And I should see the end time of the shift
	And I should see the contract time of the shift
