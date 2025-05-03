using Primrose.src.Sql;

namespace Primrose.Tests;

public class SqlTableTests {
    [Fact]
    public void CreateTableTest() {
        var db = new SqlEngine(debug: false);
        db.Setup();

        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery(
            "create table test (x int)"
        );
        Assert.NotNull(db.controller.GetTable("test"));

        db.ExecuteQuery(
            "drop table test"
        );
        Assert.Null(db.controller.GetTable("test"));
    }

    [Fact]
    public void DropTableTest() {
        var db = new SqlEngine(debug: false);
        db.Setup();

        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery(
            "drop table primrose_master"
        );

        var table = db.controller.GetTable("primrose_master");
        Assert.Null(table);
    }
}