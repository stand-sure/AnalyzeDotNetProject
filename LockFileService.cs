namespace AnalyzeDotNetProject
{
    using System.IO;

    using NuGet.Common;
    using NuGet.ProjectModel;

    public class LockFileService
    {
        public LockFile GetLockFile(string projectPath, string outputPath)
        {
            // Run the restore command
            var dotNetRunner = new DotNetRunner();
            string[] arguments = { "restore", $"\"{projectPath}\"" };
            RunStatus runStatus = dotNetRunner.Run(Path.GetDirectoryName(projectPath), arguments);

            // Load the lock file
            string lockFilePath = Path.Combine(outputPath, "project.assets.json");
            return LockFileUtilities.GetLockFile(lockFilePath, NullLogger.Instance);
        }
    }
}