namespace AnalyzeDotNetProject
{
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    /// <remarks>
    ///     Credit for the stuff happening in here goes to the https://github.com/jaredcnance/dotnet-status project
    /// </remarks>
    public class DotNetRunner
    {
        public RunStatus Run(string workingDirectory, string[] arguments)
        {
            var startInfo = new ProcessStartInfo("dotnet", string.Join(" ", arguments))
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var process = new Process();

            try
            {
                process.StartInfo = startInfo;
                process.Start();

                var output = new StringBuilder();
                var errors = new StringBuilder();
                Task outputTask = ConsumeStreamReaderAsync(process.StandardOutput, output);
                Task errorTask = ConsumeStreamReaderAsync(process.StandardError, errors);

                bool processExited = process.WaitForExit(20000);

                if (processExited == false)
                {
                    process.Kill();

                    return new RunStatus(output.ToString(), errors.ToString(), -1);
                }

                Task.WaitAll(outputTask, errorTask);

                return new RunStatus(output.ToString(), errors.ToString(), process.ExitCode);
            }
            finally
            {
                process.Dispose();
            }
        }

        private static async Task ConsumeStreamReaderAsync(TextReader reader, StringBuilder lines)
        {
            await Task.Yield();

            string line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                lines.AppendLine(line);
            }
        }
    }
}