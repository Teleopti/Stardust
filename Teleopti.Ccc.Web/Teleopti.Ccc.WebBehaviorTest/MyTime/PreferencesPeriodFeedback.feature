Feature: Preferences period feedback
	In order to know if my preferences are viable
	As an agent
	I want feedback of my preferences compared to my contract for the period



Scenario: Period feedback of contract day off
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract schedule with 2 days off
	When I view preferences
	Then I should see a message that I should have 2 days off



Scenario: Period feedback of day off preferences
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a day off preference with
	| Field | Value        |
	| Date  | 2014-04-30 |
	And I have a day off preference with
	| Field | Value        |
	| Date  | 2014-05-02 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that my preferences can result 2 days off
	And I should not see a warning for my dayoff preferences outside the target

Scenario: Period feedback of day off scheduled
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have an assigned dayoff with
         | Field | Value |
         | Date  | 2014-04-30 |
		 And I have an assigned dayoff with
         | Field | Value |
         | Date  | 2014-05-02 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that my preferences can result 2 days off

Scenario: Period feedback of day off preferences and scheduled
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a day off preference with
	| Field | Value        |
	| Date  | 2014-04-30 |
		 And I have an assigned dayoff with
         | Field | Value |
         | Date  | 2014-05-02 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that my preferences can result 2 days off



Scenario: Period feedback of contract day off tolerance
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract schedule with 2 days off
	And I have a contract with:
	| Field                      | Value  |
	| Positive day off tolerance | 1      |
	| Negative day off tolerance | 1      |
	When I view preferences
	Then I should see a message that I should have between 1 and 3 days off



Scenario: Period feedback of absence on contract schedule day off
	Given I am an agent
	And I am swedish
	And I have a scheduling period of 1 week
	And I have a contract schedule with:
	| Field              | Value |
	| Monday work day    | true  |
	| Tuesday work day   | true  |
	| Wednesday work day | true  |
	| Thursday work day  | true  |
	| Friday work day    | true  |
	| Saturday work day  | false |
	| Sunday work day    | false |
	And I have existing absence preference with
         | Field | Value |
         | Date  | 2014-05-02 |
	And I have existing absence preference with
         | Field | Value |
         | Date  | 2014-05-03 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that my preferences can result 1 days off



Scenario: Period feedback of contract time for employment type Fixed staff normal work time
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract with:
		| Field                     | Value                        |
		| Employment type           | Fixed staff normal work time |
		| Average work time per day | 8                            |
	And I have a contract schedule with 2 days off
	When I view preferences
	Then I should see a message that I should work 40 hours

Scenario: Period feedback of contract time for employment type Fixed staff day work time
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract with:
		| Field                     | Value                     |
		| Employment type           | Fixed staff day work time |
		| Average work time per day | 8                         |
	And I have existing day off preference with
         | Field | Value |
         | Date  | 2014-05-03 |
	And I have existing day off preference with
         | Field | Value |
         | Date  | 2014-05-04 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that I should work 40 hours

Scenario: Period feedback of contract time with target tolerance
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract with:
		| Field                           | Value                        |
		| Employment type                 | Fixed staff normal work time |
		| Average work time per day       | 8                            |
		| Negative target tolerance hours | 5                            |
		| Positive target tolerance hours | 5                            |
	And I have a contract schedule with 2 days off
	When I view preferences
	Then I should see a message that I should work 35 to 45 hours



Scenario: Period feedback of nothing
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a shift bag with start times 7 to 9 and end times 15 to 17
	When I view preferences
	Then I should see a message that my preferences can result in 42 to 70 hours

Scenario: Period feedback of preferences
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a shift bag with start times 7 to 9 and end times 15 to 17
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-04-28 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-04-29 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-04-30 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-05-01 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-05-02 |
	And I have existing day off preference with
         | Field | Value |
         | Date  | 2014-05-03 |
	And I have existing day off preference with
         | Field | Value      |
         | Date  | 2014-05-04 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that my preferences can result in 30 to 50 hours
	And I should not see a warning for my time preferences outside the target

Scenario: Period feedback of schedules
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a scheduled shift of 8 hours on '2014-04-28'
	And I have a scheduled shift of 8 hours on '2014-04-29'
	And I have a scheduled shift of 8 hours on '2014-04-30'
	And I have a scheduled shift of 8 hours on '2014-05-01'
	And I have a scheduled shift of 8 hours on '2014-05-02'
	And I have an assigned dayoff with
	| Field | Value      |
	| Date  | 2014-05-03 |
	And I have an assigned dayoff with
	| Field | Value      |
	| Date  | 2014-05-04 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that my preferences can result in 40 hours

Scenario: Period feedback of schedules and preferences
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a shift bag with start times 7 to 9 and end times 15 to 17
	And I have a scheduled shift of 8 hours on '2014-04-28'
	And I have a scheduled shift of 8 hours on '2014-04-29'
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-04-30 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-05-01 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-05-02 |
	And I have an assigned dayoff with
	| Field | Value      |
	| Date  | 2014-05-03 |
	And I have existing day off preference with
         | Field | Value      |
         | Date  | 2014-05-04 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that my preferences can result in 34 to 46 hours



Scenario: Period feedback of contract time absence
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract with:
		| Field                     | Value                        |
		| Average work time per day | 8                            |
	And I have a shift bag with start times 7 to 9 and end times 15 to 17
	And I have a contract schedule with 2 days off
	And I have existing absence preference with
         | Field   | Value                 |
         | Date    | 2014-04-28            |
         | Absence | Legacy common vacation absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-04-29                    |
		 | Absence | Legacy common vacation absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-04-30                    |
		 | Absence | Legacy common vacation absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-05-01                    |
		 | Absence | Legacy common vacation absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-05-02                    |
		 | Absence | Legacy common vacation absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-05-03                    |
		 | Absence | Legacy common vacation absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-05-04                    |
		 | Absence | Legacy common vacation absence |
	When I view preferences for date '2014-05-02'
	Then I should see a message that my preferences can result in 40 hours

Scenario: Period feedback of non-contract time absence
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract with:
		| Field                     | Value                        |
		| Average work time per day | 8                            |
	And I have a shift bag with start times 7 to 9 and end times 15 to 17
	And I have a contract schedule with 2 days off
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-04-28                    |
         | Absence | Legacy common absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-04-29                    |
         | Absence | Legacy common absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-04-30                     |
         | Absence | Legacy common absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-05-01                     |
         | Absence | Legacy common absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-05-02                     |
         | Absence | Legacy common absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-05-03                     |
         | Absence | Legacy common absence |
	And I have existing absence preference with
         | Field   | Value                          |
         | Date    | 2014-05-04                     |
         | Absence | Legacy common absence |
	When I view preferences for date '2014-05-02'
	Then I should see a message that my preferences can result in 0 hours

	

Scenario: Period feedback of day off preferences with warning
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have existing day off preference with
         | Field | Value      |
         | Date  | 2014-04-30 |
	And I have existing day off preference with
         | Field | Value      |
         | Date  | 2014-05-01 |
	And I have existing day off preference with
         | Field | Value      |
         | Date  | 2014-05-02 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that I should have 2 days off
	And I should see a message that my preferences can result 3 days off
	And I should see a warning for my dayoff preferences outside the target

Scenario: Period feedback of preferences with warning
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a shift bag with start times 7 to 9 and end times 15 to 17
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-04-28 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-04-29 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-04-30 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-05-01 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-05-02 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-05-03 |
	And I have existing shift category preference with
	| Field      | Value      |
	| Date | 2014-05-04 |
	When I view preferences for date '2014-05-02'
	Then I should see a message that I should work 40 hours
	And I should see a message that my preferences can result in 42 to 70 hours
	And I should see a warning for my time preferences outside the target