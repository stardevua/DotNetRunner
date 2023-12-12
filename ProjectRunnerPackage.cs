using EnvDTE;
using Microsoft.VisualStudio.Shell;
using ProjectRunner.Models;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ProjectRunner
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(ProjectRunnerPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ToolWindow1))]
    public sealed class ProjectRunnerPackage : AsyncPackage
    {
        /// <summary>
        /// ProjectRunnerPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "d7ce1c06-9061-47b5-96ed-b1636c14ac20";

        public event EventHandler OnConfigChanged;

        private EnvDTE.SolutionEvents _solutionEvents;
        private DTE _dte;

        public SolutionConfig CurrentConfig { get; private set; }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _dte = (DTE)await GetServiceAsync(typeof(DTE));
            _solutionEvents = _dte.Events.SolutionEvents;
            _solutionEvents.Opened += OnSolutionOpened;
            _solutionEvents.ProjectAdded += _solutionEvents_ProjectAdded;
            _solutionEvents.ProjectRemoved += _solutionEvents_ProjectRemoved;
            _solutionEvents.AfterClosing += _solutionEvents_AfterClosing;

            await ToolWindow1Command.InitializeAsync(this);
        }

        private void _solutionEvents_AfterClosing()
        {
            CurrentConfig = null;
            OnConfigChanged?.Invoke(this, new EventArgs());
        }

        private void _solutionEvents_ProjectRemoved(Project Project)
        {
            throw new NotImplementedException();
        }

        private void _solutionEvents_ProjectAdded(Project Project)
        {
            throw new NotImplementedException();
        }

        private void OnSolutionOpened()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_dte.Solution == null)
                return;

            string solutionDir = Path.GetDirectoryName(_dte.Solution.FullName);
            string configFilePath = Path.Combine(solutionDir, ".vs", "ProjectRunner", "solutionConfig.json");
            CurrentConfig = SolutionConfigManager.InitializeConfig(configFilePath, _dte.Solution.Projects);

            OnConfigChanged?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
