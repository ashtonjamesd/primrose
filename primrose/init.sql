create user primrose identified by 'primrose'
login user primrose identified by 'primrose'

create database master
use master

grant all privileges on master.* to primrose

create table primrose_master (
    type varchar(255),
    name varchar(255),
    sql  varchar(MAX)
)