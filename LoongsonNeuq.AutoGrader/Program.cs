// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using LoongsonNeuq.AutoGrader;
using LoongsonNeuq.Common;

Console.WriteLine("Hello, World!");

var runner = new StepRunner(new GradingStep
{
    Title = "Hello",
    Command = "sleep 5 && echo Hello",
    Timeout = 1,
    Score = 10
}, new Logger());

var result = await runner.RunAsync();

Console.WriteLine(result.StandardOutput);