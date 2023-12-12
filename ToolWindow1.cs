using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace ProjectRunner
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("24967193-0e68-4255-814b-c2afb80d9e91")]
    public class ToolWindow1 : ToolWindowPane
    {
        private readonly ToolWindow1Control _control;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1"/> class.
        /// </summary>
        public ToolWindow1() : base(null)
        {
            this.Caption = "DotNetRunner";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            _control = new ToolWindow1Control();
            this.Content = _control;
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            var package = (ProjectRunnerPackage)base.Package;

            _control.Initialize(package);
        }
    }
}
