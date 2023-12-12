using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.RpcContracts.Build;
using Microsoft.VisualStudio.Shell;
using ProjectRunner.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Process = System.Diagnostics.Process;

namespace ProjectRunner
{
    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class ToolWindow1Control : UserControl
    {
        private DTE2 _dte;
        private ProjectRunnerPackage _package;
        private readonly ProjectManager _projectManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Control"/> class.
        /// </summary>
        public ToolWindow1Control()
        {
            this.InitializeComponent();

            ThreadHelper.ThrowIfNotOnUIThread();

            _dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            _projectManager = new ProjectManager(_dte);

        }

        public void Initialize(ProjectRunnerPackage package)
        {
            _package = package;

            package.OnConfigChanged += Package_OnConfigChanged;
        }

        private void Package_OnConfigChanged(object sender, EventArgs e)
        {
            ProjectsList.ItemsSource = ((ProjectRunnerPackage)sender).CurrentConfig.LaunchProfiles;
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "ToolWindow1");
        }
        private void LaunchProfile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var project = button.DataContext as LaunchProfileConfig;


            var processId = RunDotNetProcess(project);

            //AttachDebuger(processId);
        }



        private int RunDotNetProcess(LaunchProfileConfig launchProfile)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{launchProfile.ProjectFullName}\" --launch-profile \"{launchProfile.ProfileName}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                //CreateNoWindow = true                
            };


            var logPaneName = $"DotNetRunner - {launchProfile.ProjectName}";

            using (Process process = new Process { StartInfo = startInfo })
            {                
                process.Start();
                int processId = process.Id;

                // Асинхронне читання виводу
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var window = _dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                var outputWindow = (OutputWindow)window.Object;
                OutputWindowPane pane = outputWindow.OutputWindowPanes.Add(logPaneName);
                pane.Activate();


                WriteToOutputWindow(pane, "Starting run");


                process.OutputDataReceived += (sender, args) => WriteToOutputWindow(pane, args.Data);
                process.ErrorDataReceived += (sender, args) => WriteToOutputWindow(pane, args.Data);

                return processId;
            }
        }

        private void AttachDebuger(int processId)
        {
            var processToAttachTo = _dte.Debugger.LocalProcesses
                    .OfType<EnvDTE.Process>()
                    .FirstOrDefault(process =>
                    {
                        return process.ProcessID == processId;
                    });

            if (processToAttachTo != null)
            {
                processToAttachTo.Attach();
            }

        }

        private void WriteToOutputWindow(OutputWindowPane pane, string data)
        {
            //if (string.IsNullOrEmpty(data))
            //{
            //    return;
            //}
            //_package.JoinableTaskFactory.Run(() =>
            //{
            //    //ThreadHelper.ThrowIfNotOnUIThread();
            //    pane.OutputString(data + Environment.NewLine);
            //    return Task.FromResult(0);
            //});
        }



        private void StopDebug_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE.Debugger debugger = _dte.Debugger;
            foreach (EnvDTE.Process process in debugger.DebuggedProcesses)
            {
                // Логіка для ідентифікації потрібного процесу
            }

        }
    }
}