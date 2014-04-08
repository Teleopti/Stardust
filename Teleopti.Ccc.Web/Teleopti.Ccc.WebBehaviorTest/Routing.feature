Feature: Routing
	In order make it easy to browse to the site
	As a user
	I want to be redirected to the correct locations

Background:
	Given there is a role with
	| Field                    | Value               |
	| Name                     | Access to all areas |
	| Access to mobile reports | true                |
	| Access to mytime web     | true                |
	| Access to anywhere       | true                |
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Access to report only |
	| Access to mobile reports | true                  |
	| Access to mytime web     | false                 |
	| Access to anywhere       | false                 |
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Access to mytime only |
	| Access to mobile reports | false                 |
	| Access to mytime web     | true                  |
	| Access to anywhere       | false                 |
	Given there is a role with
	| Field                    | Value                   |
	| Name                     | Access to anywhere only |
	| Access to mobile reports | false                   |
	| Access to mytime web     | false                   |
	| Access to anywhere       | true                    |
	Given there is a role with
	| Field                    | Value                         |
	| Name                     | Access to all except anywhere |
	| Access to mobile reports | true                          |
	| Access to mytime web     | true                          |
	| Access to anywhere       | false                         |

Scenario: Browse to root
	Given I am not signed in
	When I navigate to the site's root
	Then I should see the global sign in page
	
Scenario: Browse to root and sign in to menu when no Anywhere permission
	Given I have the role 'Access to all except anywhere'
	When I navigate to the site's root
	And I select application logon data source
	And I sign in
	Then I should see the global menu
 
Scenario: Browse to root and sign in to MyTime
	Given I have the role 'Access to mytime only'
	When I navigate to the site's root
	And I select application logon data source
	And I sign in
	Then I should see MyTime

Scenario: Browse to root and sign in to Mobile Reports
	Given I have the role 'Access to report only'
	When I navigate to the site's root
	And I select application logon data source
	And I sign in
	Then I should see Mobile Reports
	
Scenario: Browse to root and sign in to Anywhere
	Given I have the role 'Access to anywhere only'
	When I navigate to the site's root
	And I select application logon data source
	And I sign in
	Then I should see Anywhere

Scenario: Browse to root and always sign in to Anywhere if have Anywhere permission
	Given I have the role 'Access to all areas'
	When I navigate to the site's root
	And I select application logon data source
	And I sign in
	Then I should see Anywhere

Scenario: Browse to MyTime and sign in
	Given I have the role 'Access to all areas'
	When I navigate to MyTime
	And I select application logon data source
	And I sign in
	Then I should see MyTime

Scenario: Browse to Mobile Reports and sign in
	Given I have the role 'Access to all areas'
	When I navigate to Mobile Reports
	And I select application logon data source
	And I sign in
	Then I should see Mobile Reports
	
Scenario: Browse to Anywhere and sign in
	Given I have the role 'Access to all areas'
	When I navigate to Anywhere
	And I select application logon data source
	And I sign in
	Then I should see Anywhere