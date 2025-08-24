using System;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ReportGenerator;

/// <summary>
/// This is the main build file for the project.
/// This partial is responsible for the build process.
/// </summary>
partial class Build
{
    private static AbsolutePath CoverageDirectory => RootDirectory / "coverage";
    private static AbsolutePath CoverageResultFile => CoverageDirectory / "coverage.xml";
    private static AbsolutePath CoverageReportDirectory => CoverageDirectory / "report";
    private static AbsolutePath CoverageReportSummaryDirectory => CoverageReportDirectory / "Summary.txt";

    private Target Test => td => td
        .After(Compile)
        .Produces(CoverageResultFile)
        .Executes(() =>
            DotNetTasks.DotNetTest(settings => settings
                .SetConfiguration(Configuration)

                // Test Coverage
                // .SetResultsDirectory(CoverageDirectory)
                // .SetCoverletOutput(CoverageResultFile)
                // .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                // .SetExcludeByFile("**/*.g.cs") // Exclude source generated files
                // .EnableCollectCoverage()

                // New Microsoft.Testing.Extensions.CodeCoverage CLI commands
                .AddProcessAdditionalArguments("--")
                .AddProcessAdditionalArguments("--coverage")
                .AddProcessAdditionalArguments("--coverage-output-format cobertura")
                .AddProcessAdditionalArguments($"--coverage-output '{CoverageResultFile}'")
            )
        );

    public Target TestReport => td => td
        .DependsOn(Test)
        .Consumes(Test, CoverageResultFile)
        .Executes(() =>
        {
            _ = CoverageReportDirectory.CreateDirectory();
            _ = ReportGeneratorTasks.ReportGenerator(s => s
                .SetTargetDirectory(CoverageReportDirectory)
                .SetReportTypes(ReportTypes.Html, ReportTypes.TextSummary)
                .SetReports(CoverageResultFile)
            );
            var summaryText = CoverageReportSummaryDirectory.ReadAllLines();
        });
}