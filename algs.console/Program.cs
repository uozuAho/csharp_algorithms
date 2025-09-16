using System.Reflection;
using algs;

if (args.Length != 1)
{
    Console.WriteLine("Usage: algs.console <argument>");
    return;
}

var type = ClassFinder.FindClass(args[0]);

if (type == null)
{
    Console.WriteLine($"Class '{args[0]}' not found.");
    return;
}

var mainMethod = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
if (mainMethod == null)
{
    Console.WriteLine($"No Main method found in class '{type.FullName}'.");
    return;
}

var methodArgs = args.Skip(1).ToArray();
mainMethod.Invoke(null, [methodArgs]);

internal static class ClassFinder
{
    public static Type? FindClass(string name)
    {
        var algsAssembly = Assembly.GetAssembly(typeof(QuickFindUf));
        if (algsAssembly == null) throw new InvalidOperationException("Algs assembly not found");

        var matches = algsAssembly.GetTypes()
            .Where(t =>
                string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.FullName, name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            0 => null,
            > 1 => throw new AmbiguousMatchException($"Found {matches.Count} matches for {name}"),
            _ => matches[0]
        };
    }
}



