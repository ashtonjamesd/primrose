create database test
use test

create table myTable(
    fieldF varchar(1) not null,
    fieldA varchar(1),
    fieldB varchar(1),
    fieldC varchar(2) not null unique
)

insert into myTable(fieldB, fieldA, fieldF, fieldC)
values ('c', 'a', 'a', 'c'),
('c', 'dddd', 'a', 'a')

select * from myTable