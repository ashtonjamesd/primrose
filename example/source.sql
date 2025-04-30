create database test
use test

create table myTable(
    fieldA boolean,
    fieldB boolean,
    fieldC boolean
)

insert into myTable(fieldA, fieldB, fieldC)
values (null, null, null)
-- (null, null, null),
-- (null, null, null),
-- (null, null, null),
-- (null, null, null),
-- (null, null, null),
-- (null, null, null),
-- (null, null, null)

select * from myTable