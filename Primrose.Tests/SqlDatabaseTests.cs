using Primrose.src.Sql;

namespace Primrose.Tests;

public class SqlDatabaseTests {

    [Fact]
    public void CreateDatabaseTest() {
        var db = new SqlEngine(debug: true);
        db.Setup();

        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        Assert.Null(db.controller.GetDatabase("test"));

        db.ExecuteQuery(
            "create database test"
        );
        Assert.NotNull(db.controller.GetDatabase("test"));
    }

    [Fact]
    public void DropDatabaseTest() {
        var db = new SqlEngine(debug: false)
            .Setup();

        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery(
            "drop database master"
        );

        Assert.Null(db.controller.Database);
    }

    [Fact]
    public void CreateAndDropDatabaseTest() {
        var db = new SqlEngine(debug: false)
            .Setup();

        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));


        db.ExecuteQuery(
            "create database test"
        );
        Assert.NotNull(db.controller.GetDatabase("test"));

        db.ExecuteQuery(
            "drop database test"
        );
        Assert.Null(db.controller.GetDatabase("test"));
    }
}