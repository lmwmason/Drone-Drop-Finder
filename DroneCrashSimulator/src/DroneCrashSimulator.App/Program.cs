using DroneCrashSimulator.App;
using DroneCrashSimulator.App.Modes;
using DroneCrashSimulator.Io.CommandLine;

var options = CommandLineParser.Parse(args);

if (options.Mode == ExecutionMode.HeadlessBatch)
{
    var (_, headless) = CompositionRoot.Build();
    headless.Run(options);
}
else
{
    new LibraryGeneratorMode().Run();
}
