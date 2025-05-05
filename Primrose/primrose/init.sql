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

create user test_user identified by 'test'
grant create, insert, select on db.* to test_user
login user test_user identified by 'test'

create database db
use db