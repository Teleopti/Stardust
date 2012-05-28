Feature: Preferences validation
	In order to know if my preferences are viable
	As an agent
	I want validation of my preferences compared to my contract



Scenario: Period summary of day off preferences
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract schedule with 2 days off
	And I have a day off preference on weekday 3
	And I have a day off preference on weekday 5
	When I view preferences
	Then I should see a message that I should have 2 days off
	And I should see a message that my preferences can result 2 days off

Scenario: Period summary of day off scheduled
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract schedule with 2 days off
	And I have a day off scheduled on weekday 3
	And I have a day off scheduled on weekday 5
	When I view preferences
	Then I should see a message that I should have 2 days off
	And I should see a message that my preferences can result 2 days off

Scenario: Period summary of day off preferences and scheduled
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract schedule with 2 days off
	And I have a day off preference on weekday 3
	And I have a day off scheduled on weekday 5
	When I view preferences
	Then I should see a message that I should have 2 days off
	And I should see a message that my preferences can result 2 days off



Scenario: Period summary with day off tolerance
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract schedule with 2 days off
	And I have a contract with a day off tolerance of negative 1 days
	And I have a contract with a day off tolerance of positive 1 days
	And I have a day off preference on weekday 3
	And I have a day off preference on weekday 5
	When I view preferences
	Then I should see a message that I should have between 1 and 3 days off
	And I should see a message that my preferences can result 2 days off



Scenario: Period summary of contract time for employment type Fixed staff normal work time
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract with employment type Fixed staff normal work time
	And I have a contract with an 8 hour average work time per day
	And I have a contract schedule with 2 days off
	When I view preferences
	Then I should see a message that I should work 40 hours

Scenario: Period summary of contract time for employment type Fixed staff day work time
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract with employment type Fixed staff day work time
	And I have a contract with an 8 hour average work time per day
	And I have a day off preference on weekday 6
	And I have a day off preference on weekday 7
	When I view preferences
	Then I should see a message that I should work 40 hours

Scenario: Period summary of contract time for employment type Fixed staff period work time
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract with employment type Fixed staff period work time
	And I have a contract with an 8 hour average work time per day
	And I have a contract schedule with 2 days off
	When I view preferences
	Then I should see a message that I should work 40 hours

Scenario: Period summary of contract time with target tolerance
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract with employment type Fixed staff normal work time
	And I have a contract with an 8 hour average work time per day
	And I have a contract with a target tolerance of negative 5 hours
	And I have a contract with a target tolerance of positive 5 hours
	And I have a contract schedule with 2 days off
	When I view preferences
	Then I should see a message that I should work 35 to 45 hours



Scenario: Period summary of preferences
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a shift bag with start times 7:00 to 9:00 and end times 15:00 to 17:00
	And I have a shift category preference on weekday 1
	And I have a shift category preference on weekday 2
	And I have a shift category preference on weekday 3
	And I have a shift category preference on weekday 4
	And I have a shift category preference on weekday 5
	And I have a day off preference on weekday 6
	And I have a day off preference on weekday 7
	When I view preferences
	Then I should see a message that my preferences can result in 30 to 50 hours

Scenario: Period summary of schedules
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a scheduled shift of 8 hours on weekday 1
	And I have a scheduled shift of 8 hours on weekday 2
	And I have a scheduled shift of 8 hours on weekday 3
	And I have a scheduled shift of 8 hours on weekday 4
	And I have a scheduled shift of 8 hours on weekday 5
	And I have a scheduled day off on weekday 6
	And I have a scheduled day off on weekday 7
	When I view preferences
	Then I should see a message that my preferences can result in 40 hours

Scenario: Period summary of schedules and preferences
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a shift bag with start times 7:00 to 9:00 and end times 15:00 to 17:00
	And I have a scheduled shift of 8 hours on weekday 1
	And I have a scheduled shift of 8 hours on weekday 2
	And I have a shift category preference on weekday 3
	And I have a shift category preference on weekday 4
	And I have a shift category preference on weekday 5
	And I have a scheduled day off on weekday 6
	And I have a day off preference on weekday 7
	When I view preferences
	And I should see a message that my preferences can result in 34 to 46 hours

