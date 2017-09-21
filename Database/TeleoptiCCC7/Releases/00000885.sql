/* Remove deleted applicationfunctions*/
DELETE FROM ApplicationFunctionInRole where ApplicationFunction in(
select id from ApplicationFunction where IsDeleted = 1)

DELETE FROM ApplicationFunction where IsDeleted = 1
