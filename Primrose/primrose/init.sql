create user primrose identified by 'primrose'
login user primrose identified by 'primrose'

create database master
use master

grant all privileges on master.* to primrose 
with grant option

create table primrose_master (
    type varchar(255),
    name varchar(255),
    sql  varchar(MAX)
)

create table test (x varchar(MAX), y int, z int)

insert into test (x, y, z)
values ('asdasds', 1, 2), 
('a', 32, 1), 
('asd', 5, 8)

update test
set x = 'a', z = 23

select * from test