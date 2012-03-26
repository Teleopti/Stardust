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
	And I have preference with day off
	When I view preferences
	Then I should see no feedback

Scenario: Feedback for a day with absence preference
	Given I am an agent
	And I have a shift bag
	And I have preference with absence
	When I view preferences
	Then I should see no feedback

Scenario: Feedback for a day with shift category preference
	Given I am an agent
	And I have a shift bag
	And I have preference with shift category AM
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts of category AM
	And I should see the end time boundry for the shift bag's shifts of category AM
	And I should see the minimum contract time for the shift bag's shifts of category AM
	And I should see the maximum contract time for the shift bag's shifts of category AM

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
	And I have a shift bag
	And I have a preference with work time limitation between 7 and 9 hours
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts matching the preference
	And I should see the end time boundry for the shift bag's shifts matching the preference
	And I should see the minimum contract time for the shift bag's shifts matching the preference
	And I should see the maximum contract time for the shift bag's shifts matching the preference

Scenario: Feedback for a day with lunch start time limitation preference
	Given I am an agent
	And I have a shift bag
	And I have a preference with lunch start time limitation between 11:00 and 12:00
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts matching the preference
	And I should see the end time boundry for the shift bag's shifts matching the preference
	And I should see the minimum contract time for the shift bag's shifts matching the preference
	And I should see the maximum contract time for the shift bag's shifts matching the preference

Scenario: Feedback for a day with lunch end time limitation preference
	Given I am an agent
	And I have a shift bag
	And I have a preference with lunch end time limitation between 12:00 and 13:00
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts matching the preference
	And I should see the end time boundry for the shift bag's shifts matching the preference
	And I should see the minimum contract time for the shift bag's shifts matching the preference
	And I should see the maximum contract time for the shift bag's shifts matching the preference

Scenario: Feedback for a day with lunch length limitation preference
	Given I am an agent
	And I have a shift bag
	And I have a preference with lunch length limitation between 1 and 2 hours
	When I view preferences
	Then I should see the start time boundry for the shift bag's shifts matching the preference
	And I should see the end time boundry for the shift bag's shifts matching the preference
	And I should see the minimum contract time for the shift bag's shifts matching the preference
	And I should see the maximum contract time for the shift bag's shifts matching the preference




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
