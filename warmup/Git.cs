// Copyright 2007-2010 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using System.Diagnostics;
using warmup.settings;

namespace warmup
{
    public class Git : IExporter
    {
        private static void RunGitCommandInExternalProcess(string command, string workingDirectoryOrNull)
        {
            command = string.Format(" /c {0}", command);
            var psi = new ProcessStartInfo("cmd", command)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            if (workingDirectoryOrNull != null)
            {
                psi.WorkingDirectory = workingDirectoryOrNull;
            }

            //todo: better error handling
            Console.WriteLine("Running: {0} {1}", psi.FileName, psi.Arguments);
            string output, error = "";
            using (var p = Process.Start(psi))
            {
                output = p.StandardOutput.ReadToEnd();
                error = p.StandardError.ReadToEnd();
            }

            Console.WriteLine(output);
            Console.WriteLine(error);
        }

        public void Export(string sourceControlWarmupLocation, string templateName, TargetDir targetDir)
        {
            var gitUri = WarmupConfiguration.settings.SourceControlWarmupLocation + templateName;
            Console.WriteLine("git exporting to: {0}", targetDir.FullPath);

            var separationCharacters = new[] {".git"};
            var piecesOfPath = gitUri.Split(separationCharacters, StringSplitOptions.RemoveEmptyEntries);
            if (piecesOfPath.Length <= 0) return;

            var sourceLocationToGit = piecesOfPath[0] + ".git";

            var cloneCommand = string.Format("git clone {0} {1}", sourceLocationToGit, targetDir.FullPath);
            RunGitCommandInExternalProcess(cloneCommand, null);

            if (WarmupConfiguration.settings.GitBranch != null)
            {
                var checkoutCommand = string.Format("git checkout {0}", WarmupConfiguration.settings.GitBranch);
                RunGitCommandInExternalProcess(checkoutCommand, targetDir.FullPath);
            }

            var exportTemplateName = piecesOfPath.Length > 1 ? piecesOfPath[1] : WarmupConfiguration.settings.DefaultTemplate;
            new GitTemplateExtractor(targetDir, exportTemplateName).Extract();
        }
    }
}