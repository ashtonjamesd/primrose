using Primrose.src.Sql;

namespace Primrose.Tests.src;

public class InsertIntoTests {
    [Fact]
    public void InsertAndSelectTest() {
        var db = new SqlEngine()
            .Setup();
        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetDatabase("master"));
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery("create table test (x int, y varchar(255))");
        db.ExecuteQuery("insert into test (x, y) values (1, 'hello'), (2, 'world'), (3, '!')");
        var result = db.ExecuteQuery("select * from test");

        Assert.Equal(3, result.RowsAffected);

        Assert.NotNull(result.Table);

        Assert.Equal(1, result.Table.Rows[0]["x"]);
        Assert.Equal("hello", result.Table.Rows[0]["y"]);

        Assert.Equal(2, result.Table.Rows[1]["x"]);
        Assert.Equal("world", result.Table.Rows[1]["y"]);

        Assert.Equal(3, result.Table.Rows[2]["x"]);
        Assert.Equal("!", result.Table.Rows[2]["y"]);
    }

    [Fact]
    public void InsertWithNullsTest() {
        var db = new SqlEngine()
            .Setup();
        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetDatabase("master"));
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery("create table test (x int, y varchar(255))");
        db.ExecuteQuery("insert into test (x, y) values (null, null)");

        var result = db.ExecuteQuery("select * from test");
        Assert.NotNull(result.Table);
        Assert.Single(result.Table.Rows);
        
        Assert.Null(result.Table.Rows[0]["x"]);
        Assert.Null(result.Table.Rows[0]["y"]);
    }

    [Fact]
    public void InsertWithoutAllColumnsTest() {
        var db = new SqlEngine()
            .Setup();
        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetDatabase("master"));
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery("create table test (x int, y varchar(255))");
        db.ExecuteQuery("insert into test (x) values (99)");

        var result = db.ExecuteQuery("select * from test");
        Assert.NotNull(result.Table);
        Assert.Single(result.Table.Rows);

        Assert.Equal(99, result.Table.Rows[0]["x"]);
        Assert.Null(result.Table.Rows[0]["y"]);
    }
}