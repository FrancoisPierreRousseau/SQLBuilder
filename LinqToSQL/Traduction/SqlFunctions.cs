namespace LinqToSQL.Traduction;


public static class SqlFunctions
{
    public static int Count(string column) =>
        throw new NotSupportedException("This method is only meant to be parsed in expression trees.");

    public static int Sum(string column) =>
        throw new NotSupportedException("This method is only meant to be parsed in expression trees.");

    public static int Avg(string column) =>
        throw new NotSupportedException("This method is only meant to be parsed in expression trees.");
}



