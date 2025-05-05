using Primrose.src.Sql;

namespace Primrose.Tests.src;

public class SqlTests {
    [Fact]
    public void SqlTest() {
        var db = new SqlEngine()
            .Setup();
        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetDatabase("master"));
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery("create user test_user identified by 'test'");

        db.ExecuteQuery("grant create, insert, select on db.* to test_user");

        db.ExecuteQuery("login user test_user identified by 'test'");

        db.ExecuteQuery("create database db");
        db.ExecuteQuery("use db");

        db.ExecuteQuery(@"
            create table users (
                id int,
                username varchar(255),
                password varchar(255)
            )
        ");

        db.ExecuteQuery(@"
            insert into users (id, username, password)
            values (1, 'alice', 'secret'), (2, 'bob', 'hunter2')
        ");

        var result = db.ExecuteQuery("select * from users");
        Assert.NotNull(result.Table);
        Assert.Equal(2, result.RowsAffected);

        Assert.Equal("alice", result.Table.Rows[0]["username"]);
        Assert.Equal("hunter2", result.Table.Rows[1]["password"]);


        db.ExecuteQuery("login user primrose identified by 'primrose'");
        db.ExecuteQuery("use db");

        db.ExecuteQuery("drop table users");
    }
}
