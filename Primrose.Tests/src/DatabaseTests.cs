using Primrose.src.Sql;

namespace Primrose.Tests.src;

public class DatabaseTests {
    [Fact]
    public void CreateAndDropDatabaseTest() {
        var db = new SqlEngine()
            .Setup();
        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetDatabase("master"));
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery("create database test");
        Assert.NotNull(db.controller.GetDatabase("test"));

        db.ExecuteQuery("drop database test");
        Assert.Null(db.controller.GetDatabase("test"));
    }

    [Fact]
    public void UseDatabaseTest() {
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

        db.ExecuteQuery("create table test (x int)");
        db.ExecuteQuery("insert into test (x) values (1)");

        var result = db.ExecuteQuery("select * from test");
        Assert.Equal(1, result.RowsAffected);
    }

}