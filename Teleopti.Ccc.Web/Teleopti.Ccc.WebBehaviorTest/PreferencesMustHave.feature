Feature: Preferences must-haves
	In order to view and submit when I prefer to work
	As an agent
	I want to view and submit my work preferences

Background:
	Given there is a role with
	| Field                    | Value             |
	| Name                     | Access to mytime  |
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
	Given I have the role 'Access to mytime'
	And I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | true       |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	Then I should see preference
	| Field     | Value      |
	| Date      | 2012-08-12 |
	| Must have | true       |

Scenario: Set must have on preference
	Given I have the role 'Access to mytime'
	And I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | false      |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click set must have button
	Then I should see preference
	| Field     | Value      |
	| Date      | 2012-08-23 |
	| Must have | true       |

Scenario: Set must have on empty day should do nothing
	Given I have the role 'Access to mytime'
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click set must have button
	Then I should see preference
	| Field     | Value      |
	| Date      | 2012-08-23 |
	| Must have | false      |

Scenario: Remove must have from preference
	Given I have the role 'Access to mytime'
	And I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | true       |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click remove must have button
	Then I should see preference
	| Field     | Value      |
	| Date      | 2012-08-23 |
	| Must have | false      |

Scenario: See available must haves
	Given I have the role 'Access to mytime'
	When I view preferences for date '2012-08-23'
	Then I should see I have '1' available must have

Scenario: Decrement available must haves on set
	Given I have the role 'Access to mytime'
	And I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | false      |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click set must have button
	Then I should see I have '0' available must have

Scenario: Increment available must haves on remove
	Given I have the role 'Access to mytime'
	And I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | true      |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-23'
	And I click remove must have button
	Then I should see I have '1' available must have

Scenario: Disallow setting too many must haves
	Given I have the role 'Access to mytime'
	And I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-23 |
	| Must have      | true       |
	| Shift category | Late       |
	And I have a preference with
	| Field          | Value      |
	| Date           | 2012-08-24 |
	| Must have      | false      |
	| Shift category | Late       |
	When I view preferences for date '2012-08-23'
	And I select day '2012-08-24'
	And I click set must have button
	Then I should see I have '0' available must have
	And I should see preference
	| Field     | Value      |
	| Date      | 2012-08-36 |
	| Must have | true       |
	And I should see preference
	| Field     | Value      |
	| Date      | 2012-08-24 |
	| Must have | false      |
