using Primrose.src.Tokenize;

namespace Primrose.src.Parse;

internal abstract class SqlType {}

internal sealed class SqlVarchar : SqlType {
    public int MaxChars { get; set; }

    public override string ToString() {
        return $"varchar({MaxChars})";
    }
}

internal sealed class SqlBoolean : SqlType {

    public override string ToString() {
        return "boolean";
    }
}
internal sealed class SqlInt : SqlType {

    public override string ToString() {
        return "int";
    }
}
internal sealed class SqlChar : SqlType {
    public int MaxChars { get; set; }

    public override string ToString() {
        return $"char({MaxChars})";
    }
}

internal sealed class SqlUnknown : SqlType {
    public required Token Item { get; set; }

    public override string ToString() {
        return "unknown";
    }
}