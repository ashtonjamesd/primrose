create database test
use test

create table myTable(
    intField int,
    charField varchar
)

insert into myTable(intField, charField)
values ('a', 'a'), ('c', 'd')

select * from myTable