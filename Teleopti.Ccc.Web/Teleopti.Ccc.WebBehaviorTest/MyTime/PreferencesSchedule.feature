Feature: Preferences schedule
	In order to view and submit when I prefer to work
	As an agent
	I want to see when I have been scheduled already

Scenario: See scheduled shift
	Given I am an agent
	And I have an assigned shift with
         | Field   | Value                          |
         | Date    | 2014-05-02                    |
	And My schedule is published
	When I view preferences for date '2014-05-02'
	Then I should see my assigned shift for '2014-05-02'

Scenario: Can not add preference on scheduled day
	Given I am an agent
	And I have an assigned shift with
         | Field   | Value                          |
         | Date    | 2014-05-02                    |
	And My schedule is published
	When I view preferences for date '2014-05-02'
	Then I should see my assigned shift for '2014-05-02'
	And I should not be able to add preference for '2014-05-02'

Scenario: See scheduled dayoff
	Given I am an agent
	And I have an assigned dayoff with
         | Field   | Value                          |
         | Date    | 2014-05-02                    |
	And My schedule is published
	When I view preferences for date '2014-05-02'
	Then I should see the assigned dayoff for '2014-05-02'

Scenario: See scheduled absence
	Given I am an agent
	And I have an assigned shift with
         | Field   | Value                          |
         | Date    | 2014-05-02                    |
	And I have a full-day absence with
         | Field   | Value                          |
         | Date    | 2014-05-02                    |
	And My schedule is published
	When I view preferences for date '2014-05-02'
	Then I should see the assigned absence for '2014-05-02'

Scenario: See scheduled absence when no underlying shift
	Given I am an agent
	And I have a full-day absence with
	| Field   | Value                          |
         | Date    | 2014-05-02                    |
	And My schedule is published
	When I view preferences for date '2014-05-02'
	Then I should see the assigned absence for '2014-05-02'

Scenario: See scheduled absence on schedule dayoff
	Given I am an agent
	And I have an assigned dayoff with
	| Field   | Value                          |
         | Date    | 2014-05-02                    |
	And I have a full-day absence with
	| Field | Value      |
	| Date  | 2014-05-02 |
	And My schedule is published
	When I view preferences for date '2014-05-02'
	Then I should see the assigned absence for '2014-05-02'

Scenario: See scheduled absence on contract dayoff
	Given I am an agent that has a dayoff today according to my contract
	And I have a full-day absence with
	| Field   | Value                          |
         | Date    | 2014-05-02                    |
	And My schedule is published
	When I view preferences for date '2014-05-02'
	Then I should see the assigned absence for '2014-05-02'
