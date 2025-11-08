using BlueDotBrigade.Analyzers.Dsl;

namespace BlueDotBrigade.Analyzers.Tool;

internal static class Program
{
    private const string GenerateCommand = "generate-dsl";

    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0];
        if (IsHelp(command))
        {
            PrintUsage();
            return 0;
        }

        if (string.Equals(command, GenerateCommand, StringComparison.OrdinalIgnoreCase))
        {
            var commandArgs = args.Length > 1 ? args[1..] : Array.Empty<string>();
            return ExecuteGenerateCommand(commandArgs);
        }

        Console.Error.WriteLine($"Unknown command '{command}'.");
        PrintUsage();
        return 1;
    }

    private static int ExecuteGenerateCommand(string[] args)
    {
        var outputPath = DslDefaults.DefaultDslFileName;
        var writeToStdOut = false;
        var overwrite = false;

        for (var i = 0; i < args.Length; i++)
        {
            var current = args[i];
            if (IsHelp(current))
            {
                PrintGenerateUsage();
                return 0;
            }

            switch (current)
            {
                case "--output":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--output option requires a value.");
                        return 1;
                    }

                    outputPath = args[++i];
                    break;
                case "--stdout":
                    writeToStdOut = true;
                    break;
                case "--force":
                    overwrite = true;
                    break;
                default:
                    Console.Error.WriteLine($"Unrecognized option '{current}'.");
                    PrintGenerateUsage();
                    return 1;
            }
        }

        if (writeToStdOut)
        {
            Console.Out.WriteLine(DslDefaults.DefaultDslXml);
            return 0;
        }

        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!overwrite && File.Exists(fullPath))
        {
            Console.Error.WriteLine($"File '{fullPath}' already exists. Use --force to overwrite it or --stdout to print.");
            return 1;
        }

        File.WriteAllText(fullPath, DslDefaults.DefaultDslXml);
        Console.Out.WriteLine($"Sample DSL written to '{fullPath}'.");
        return 0;
    }

    private static bool IsHelp(string value)
        => string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "help", StringComparison.OrdinalIgnoreCase);

    private static void PrintUsage()
    {
        Console.Out.WriteLine("Usage: bdb-analyzers <command> [options]");
        Console.Out.WriteLine();
        Console.Out.WriteLine("Commands:");
        Console.Out.WriteLine($"  {GenerateCommand}    Generate the sample DSL file used by the analyzer.");
        Console.Out.WriteLine();
        Console.Out.WriteLine($"Run 'bdb-analyzers {GenerateCommand} --help' for command-specific options.");
    }

    private static void PrintGenerateUsage()
    {
        Console.Out.WriteLine($"Usage: bdb-analyzers {GenerateCommand} [--output <path>] [--stdout] [--force]");
        Console.Out.WriteLine();
        Console.Out.WriteLine("Options:");
        Console.Out.WriteLine($"  --output <path>  Destination file path. Defaults to '{DslDefaults.DefaultDslFileName}'.");
        Console.Out.WriteLine("  --stdout         Print the XML to standard output instead of writing a file.");
        Console.Out.WriteLine("  --force          Overwrite the destination file if it already exists.");
    }
}
