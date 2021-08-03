namespace AnalyzeDotNetProject
{
    using System;
    using System.Linq;

    using NuGet.LibraryModel;
    using NuGet.Packaging.Core;
    using NuGet.ProjectModel;

    internal class Program
    {
        private static int maxIndentLevel = 2;

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Please provide a path to the project to analyze.");

                return;
            }

            string projectPath = args[0];

            if (args.Length > 1)
            {
                int.TryParse(args[1], out Program.maxIndentLevel);
            }

            var dependencyGraphService = new DependencyGraphService();
            DependencyGraphSpec dependencyGraph = dependencyGraphService.GenerateDependencyGraph(projectPath);

            foreach (PackageSpec project in dependencyGraph.Projects.Where(spec => spec.RestoreMetadata.ProjectStyle == ProjectStyle.PackageReference))
            {
                var lockFileService = new LockFileService();
                LockFile lockFile = lockFileService.GetLockFile(project.FilePath, project.RestoreMetadata.OutputPath);

                Console.WriteLine(project.Name);

                foreach (TargetFrameworkInformation targetFramework in project.TargetFrameworks)
                {
                    Console.WriteLine($"  [{targetFramework.FrameworkName}]");

                    LockFileTarget lockFileTargetFramework =
                        lockFile.Targets.FirstOrDefault(target => target.TargetFramework.Equals(targetFramework.FrameworkName));

                    if (lockFileTargetFramework == null)
                    {
                        continue;
                    }

                    foreach (LibraryDependency dependency in targetFramework.Dependencies)
                    {
                        LockFileTargetLibrary projectLibrary = lockFileTargetFramework.Libraries.FirstOrDefault(library => library.Name == dependency.Name);

                        if (projectLibrary is null)
                        {
                            continue;
                        }

                        ReportDependency(projectLibrary, lockFileTargetFramework, 1);
                    }
                }
            }
        }

        private static void ReportDependency(LockFileTargetLibrary projectLibrary, LockFileTarget lockFileTargetFramework, int indentLevel)
        {
            if (projectLibrary is null)
            {
                return;
            }

            Console.Write(new string(' ', indentLevel * 2));
            Console.WriteLine($"{projectLibrary.Name}, v{projectLibrary.Version}");

            foreach (PackageDependency childDependency in projectLibrary.Dependencies)
            {
                LockFileTargetLibrary childLibrary = lockFileTargetFramework.Libraries.FirstOrDefault(library => library.Name == childDependency.Id);

                if (indentLevel >= Program.maxIndentLevel)
                {
                    return;
                }

                ReportDependency(childLibrary, lockFileTargetFramework, indentLevel + 1);
            }
        }
    }
}