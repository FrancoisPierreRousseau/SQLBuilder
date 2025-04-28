namespace LinqToSQL.Traduction;


public class SqlFunctions
{
    public static int Count(object column)
    {
        throw new NotSupportedException("This method should only be used in expressions and translated to SQL.");
    }

    public static int Sum(object column)
    {
        throw new NotSupportedException("This method should only be used in expressions and translated to SQL.");
    }

    public static int Avg(object column)
    {
        throw new NotSupportedException("This method should only be used in expressions and translated to SQL.");
    }
}
