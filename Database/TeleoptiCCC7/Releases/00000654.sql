----------------  
--Name: Maria und Jonas
--Date: 2016-01-19
--Desc: Clear those persons that still exist in readmodel table FindPerson due to a earlier bug
delete from ReadModel.FindPerson
where PersonId in (select Id from person where IsDeleted=1)