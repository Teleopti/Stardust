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
	| Access to admin web      | true                |
	Given there is a role with
	| Field                    | Value                       |
	| Name                     | Access to report only       |
	| Access to mobile reports | true                        |
	| Access to mytime web     | false                       |
	| Access to admin web      | false                       |
	Given there is a role with
	| Field                    | Value                       |
	| Name                     | Access to mytime only       |
	| Access to mobile reports | false                       |
	| Access to mytime web     | true                        |
	| Access to admin web      | false                       |
	Given there is a role with
	| Field                    | Value                       |
	| Name                     | Access to admin web only    |
	| Access to mobile reports | false                       |
	| Access to mytime web     | false                       |
	| Access to admin web      | true                        |

Scenario: Browse to root
	Given I am not signed in
	When I navigate to the site's root
	Then I should see the global sign in page

Scenario: Browse to MyTime
	Given I am not signed in
	When I navigate to MyTime
	Then I should see MyTime's sign in page

Scenario: Browse to Mobile Reports
	Given I am not signed in
	When I navigate to Mobile Reports
	Then I should see Mobile Report's sign in page
	
Scenario: Browse to root and sign in to menu
	Given I have the role 'Access to all areas'
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
	
Scenario: Browse to root and sign in to Web Admin
	Given I have the role 'Access to admin web only'
	When I navigate to the site's root
	And I select application logon data source
	And I sign in
	Then I should see Admin Web

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
	
Scenario: Browse to Web Admin and sign in
	Given I have the role 'Access to all areas'
	When I navigate to Admin Web
	And I select application logon data source
	And I sign in
	Then I should see Admin Web