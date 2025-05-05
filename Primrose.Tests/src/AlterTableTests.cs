using Primrose.src.Sql;

namespace Primrose.Tests.src;

public class AlterTableTests {
    [Fact]
    public void AlterTableDropColumnTest() {
        var db = new SqlEngine()
            .Setup();
        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetDatabase("master"));
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery("create table test (a int, b varchar(100), c int)");
        db.ExecuteQuery("insert into test (a, b, c) values (1, 'hello', 2)");
        db.ExecuteQuery("alter table test drop column b");

        var result = db.ExecuteQuery("select * from test");
        Assert.Equal(1, result.RowsAffected);
        Assert.NotNull(result.Table);

        Assert.Equal(1, result.Table.Rows[0]["a"]);
        Assert.Null(result.Table.Rows[0]["b"]);
        Assert.Equal(2, result.Table.Rows[0]["c"]);
    }

    [Fact]
    public void AlterTableAddColumnTest() {
        var db = new SqlEngine().Setup();

        db.ExecuteQuery("create table test (x int)");
        db.ExecuteQuery("insert into test (x) values (1), (2)");

        db.ExecuteQuery("alter table test add y varchar(50)");

        var result = db.ExecuteQuery("select * from test");
        Assert.Equal(2, result.RowsAffected);
        Assert.NotNull(result.Table);

        Assert.NotNull(db.controller.GetColumn(result.Table, "y"));
    }

    [Fact]
    public void AlterTableRenameColumnTest() {
        var db = new SqlEngine().Setup();

        db.ExecuteQuery("create table test (old_name int)");
        db.ExecuteQuery("insert into test (old_name) values (123)");

        db.ExecuteQuery("alter table test rename column old_name to new_name");

        var result = db.ExecuteQuery("select * from test");
        Assert.NotNull(result.Table);
        Assert.Single(result.Table.Columns);

        Assert.NotNull(db.controller.GetColumn(result.Table, "new_name"));
        Assert.Null(db.controller.GetColumn(result.Table, "old_name"));
        Assert.Equal(123, result.Table.Rows[0]["new_name"]);
    }
}