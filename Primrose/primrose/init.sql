create user primrose identified by 'primrose'
login user primrose identified by 'primrose'

create database master
use master

grant all privileges on master.* to primrose
with grant option

create table primrose_master (
    type varchar(255),
    name varchar(255),
    sql  varchar(MAX),
    test boolean default false
)

create table employees (
    id int,
    first_name varchar(50),
    last_name varchar(50),
    email varchar(100),
    department varchar(MAX)
)

insert into employees (id, first_name, last_name, email, department) values
(1, 'John', 'Doe', 'john.doe@example.com', 'Engineering'),
(2, 'Jane', 'Smith', 'jane.smith@example.com', 'Marketing'),
(3, 'Alice', 'Johnson', 'alice.j@example.com', 'HR'),
(4, 'Bob', 'Williams', 'bob.w@example.com', 'Engineering'),
(5, 'Emily', 'Davis', 'emily.d@example.com', 'Finance'),
(6, 'Michael', 'Brown', 'm.brown@example.com', 'Sales'),
(7, 'Sarah', 'Miller', 's.miller@example.com', 'IT'),
(8, 'David', 'Wilson', 'd.wilson@example.com', 'Engineering'),
(9, 'Laura', 'Moore', 'laura.moore@example.com', 'HR'),
(10, 'Daniel', 'Taylor', 'daniel.t@example.com', 'Marketing'),
(11, 'Anna', 'Anderson', 'anna.a@example.com', 'Finance'),
(12, 'James', 'Thomas', 'j.thomas@example.com', 'Sales'),
(13, 'Karen', 'Jackson', 'k.jackson@example.com', 'IT'),
(14, 'Brian', 'White', 'brian.w@example.com', 'Engineering'),
(15, 'Nancy', 'Harris', 'nancy.h@example.com', 'HR'),
(16, 'Steven', 'Martin', 'steven.m@example.com', 'Finance'),
(17, 'Rachel', 'Thompson', 'r.thompson@example.com', 'Sales'),
(18, 'Greg', 'Garcia', 'greg.g@example.com', 'Engineering'),
(19, 'Sophia', 'Martinez', 's.martinez@example.com', 'IT'),
(20, 'Kevin', 'Robinson', 'kevin.r@example.com', 'HR'),
(21, 'John', 'Doe', 'john.doe@example.com', 'Engineering'),
(22, 'Emily', 'Davis', 'emily.d@example.com', 'Finance'),
(23, 'Grace', 'Lewis', 'grace.l@example.com', 'Sales'),
(24, 'Grace', 'Lewis', 'grace.l@example.com', 'Sales'), -- duplicate
(25, 'Zoe', 'Allen', 'zoe.a@example.com', 'HR'),
(26, 'Ryan', 'Hall', 'ryan.h@example.com', 'Finance'),
(27, 'Zoe', 'Allen', 'zoe.a@example.com', 'HR'), -- duplicate
(28, 'Nathan', 'Young', 'nathan.y@example.com', 'Sales'),
(29, 'Lily', 'King', 'lily.k@example.com', 'Marketing'),
(29, 'Lily', 'King', 'lily.k@example.com', 'Marketing'),
(29, 'Lily', 'King', 'lily.k@example.com', 'Marketing'),
(29, 'Lily', 'King', 'lily.k@example.com', 'Marketing'),
(29, 'Lily', 'King', 'lily.k@example.com', 'Marketing'),
(30, 'Lily', 'King', 'lily.k@example.com', 'Marketing') -- duplicate

select distinct * from employees where email = 'lily.k@example.com'