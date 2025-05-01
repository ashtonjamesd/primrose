using Primrose.src.Tokenize;

namespace Primrose.src.Parse;

public abstract class SqlType {}

public sealed class SqlVarchar : SqlType {
    public int MaxChars { get; set; }

    public override string ToString() {
        return $"varchar({MaxChars})";
    }
}

public sealed class SqlBoolean : SqlType {

    public override string ToString() {
        return "boolean";
    }
}
public sealed class SqlInt : SqlType {

    public override string ToString() {
        return "int";
    }
}
public sealed class SqlChar : SqlType {
    public int MaxChars { get; set; }

    public override string ToString() {
        return $"char({MaxChars})";
    }
}

public sealed class SqlUnknown : SqlType {
    public required Token Item { get; set; }

    public override string ToString() {
        return "unknown";
    }
}