using System;

namespace SAaP.Core.Services.Analyze;

public class Condition
{
    private Condition()
    {
    }

    public static Condition Parse(string condition)
    {

        return new Condition();
    }

    public static bool TryParse(string condition)
    {
        try
        {
            Parse(condition);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}