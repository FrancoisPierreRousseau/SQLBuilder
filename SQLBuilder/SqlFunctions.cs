namespace SQLBuilder;
public class SqlFunctions
{
    public static string Count(object column)
    {
        if(column is string col)
            return $"COUNT({col})";
        throw new ArgumentException("Invalid column type for COUNT function.");
    }

    public static string Sum(object column)
    {
        if(column is string col)
            return $"SUM({col})";
        throw new ArgumentException("Invalid column type for SUM function.");
    }

    public static string Avg(object column)
    {
        if(column is string col)
            return $"AVG({col})";
        throw new ArgumentException("Invalid column type for AVG function.");
    }
}
