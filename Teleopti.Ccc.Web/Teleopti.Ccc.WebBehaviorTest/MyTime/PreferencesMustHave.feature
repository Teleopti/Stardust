Feature: Preferences must haves
	In order to get scheduled according specific preferences
	As an agent
	I want to stress which of my preferences are most important

Background:
	Given I have a role with
	| Field                          | Value                 |
	| Name                           | Full access to mytime |
	| Access to extended preferences | false                 |
    And there is a shift category with
    | Field | Value |
    | Name  | Late  |
    And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2012-08-26         |
	And I have a schedule period with 
	| Field                | Value      |
	| Start date           | 2012-08-20 |
	| Type                 | Week       |
	| Length               | 1          |
	| Must have preference | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-08-20 |

Scenario: See must have preference
	Given I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | true       |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	Then I should see preference
	| Field     | Value      |
	| Date      | 2012-08-23 |
	| Must have | true       |

Scenario: Set must have on preference
	Given I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | false      |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click set must have button
	Then I should see preference
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | true       |
	| Shift category | Late       |

Scenario: Set must have on empty day should do nothing
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click set must have button
	Then I should see preference
	| Field     | Value      |
	| Date      | 2012-08-23 |
	| Must have | false      |

Scenario: Remove must have from preference
	Given I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | true       |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click remove must have button
	Then I should see preference
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | false      |
	| Shift category | Late       |

Scenario: See available must haves
	When I view preferences for date '2012-08-23'
	Then I should see I have 1 available must haves

Scenario: Increment must haves on set
	Given I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | false      |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click set must have button
	Then I should see I have 1 must haves

Scenario: Decrement must haves on remove
	Given I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | true       |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click remove must have button
	Then I should see I have 0 must haves

Scenario: Disallow setting too many must haves
	Given I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | false      |
	| Shift category | Late       |
	And I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-24 |
	| Must have      | false      |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click set must have button
	Then I should see I have 1 must haves
	And I should see preference
	| Field     | Value      |
	| Date      | 2012-08-23 |
	| Must have | true       |
	When I select day '2012-08-24'
	And I click set must have button
	Then I should see preference
	| Field     | Value      |
	| Date      | 2012-08-24 |
	| Must have | false      |
