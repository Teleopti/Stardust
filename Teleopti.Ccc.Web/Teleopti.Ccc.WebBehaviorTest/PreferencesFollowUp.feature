Feature: Preferences follow up
	In order to see my schedule and my preferences side by side
	As an agent
	I can easily see where my preferences were fulfilled and where they were not

Background:
	Given I have a role with
	| Field                          | Value                 |
	| Name                           | Full access to mytime |
	| Access to extended preferences | false                 |
	And there are shift categories
	| Name  |
	| Late  |
	| Night |
	And there is a dayoff with
	| Field | Value  |
	| Name  | Dayoff |
	And there is an absence with
	| Field | Value   |
	| Name  | Illness |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2012-10-07         |
	| Available shift category   | Late               |
	| Available shift category   | Night              |
	| Available dayoff           | Dayoff             |
	| Available absence          | Illness            |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-10-01 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-10-01 |


Scenario: See that preference is fulfilled
	Given I have a preference with
	| Field      | Value      |
	| Date       | 2012-10-02 |
	| Preference | Late       |
	And I have a shift with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Late       |
	And My schedule is published
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Late       |
	| Preference     | Late       |
	| Fulfilled      | true       |

Scenario: See that preference not fulfilled
	Given I have a preference with
	| Field      | Value      |
	| Date       | 2012-10-02 |
	| Preference | Late       |
	And I have a shift with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Night      |
	And My schedule is published
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Late       |
	| Preference     | Night      |
	| Fulfilled      | false      |

Scenario: See extended indication on preference on scheduled day
	Given I have an extended preference with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| End time maximum | 20:30      |
	And I have a shift with
	| Field             | Value            |
	| StartTime         | 2012-10-02 10:00 |
	| EndTime           | 2012-10-02 20:00 |
	| ShiftCategoryName | Late             |
	And My schedule is published
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field               | Value      |
	| Date                | 2012-10-02 |
	| Shift category      | Late       |
	| Preference          | Extended   |
	| Fulfilled           | true       |
	| Extended Indication | true       |

Scenario: Display extended preference panel for preference on scheduled day
	Given I have an extended preference with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| End time maximum | 20:30      |
	And I have a shift with
	| Field             | Value            |
	| StartTime         | 2012-10-02 20:00 |
	| EndTime           | 2012-10-03 04:00 |
	| ShiftCategoryName | Night            |
	And My schedule is published
	When I view preferences for date '2012-10-02'
	And I click the extended preference indication on '2012-10-02'
	Then I should see extended preference with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| End time maximum | 20:30      |

Scenario: Display must have for preference on scheduled day
	Given I have a preference with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Late       |
	And I have a shift with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Late       |
	And My schedule is published
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field     | Value      |
	| Date      | 2012-10-02 |
	| Must have | true       |





Scenario: Dayoff preference fulfilled
	Given I have a preference with
	| Field      | Value      |
	| Date       | 2012-10-02 |
	| Preference | Dayoff     |
	And I have a shift with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Dayoff     |
	And My schedule is published
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| Shift category   | Dayoff     |
	| Preference       | Dayoff     |
	| Preference Color | Green      |

Scenario: Dayoff preference not fulfilled
	Given I have a preference with
	| Field      | Value      |
	| Date       | 2012-10-02 |
	| Preference | Dayoff     |
	And I have a shift with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Late       |
	And My schedule is published
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| Shift category   | Late       |
	| Preference       | Dayoff     |
	| Preference Color | Red        |

Scenario: Absence preference fulfilled
	Given I have a preference with
	| Field      | Value      |
	| Date       | 2012-10-02 |
	| Preference | Absence    |
	And I have a shift with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Absence    |
	And My schedule is published
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| Shift category   | Absence    |
	| Preference       | Absence    |
	| Preference Color | Green      |

Scenario: Absence preference not fulfilled
	Given I have a preference with
	| Field      | Value      |
	| Date       | 2012-10-02 |
	| Preference | Absence    |
	And I have a shift with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Late       |
	And My schedule is published
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| Shift category   | Late       |
	| Preference       | Absence    |
	| Preference Color | Red        |