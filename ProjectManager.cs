using EnvDTE;
using EnvDTE80;
//using Microsoft.Build.Construction;
using Microsoft.VisualStudio.RpcContracts.Build;
using Microsoft.VisualStudio.Shell.Interop;

public class ProjectManager
{
    private readonly DTE2 _dte;

    public ProjectManager(DTE2 dte)
    {
        _dte = dte;
    }

    public void LaunchProfile(string projectPath)
    {
        Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
        SelectProjectInSolutionExplorer(projectPath);

         _dte.ExecuteCommand("Debug.Start", "");
    }

    private void SelectProjectInSolutionExplorer(string projectPath)
    {
        Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

        //string projectPath = $"{solutionName}\\{projectName}";
        var d = _dte.ToolWindows.SolutionExplorer.SelectedItems;
        UIHierarchyItem projectItem = _dte.ToolWindows.SolutionExplorer.GetItem(projectPath);
        projectItem?.Select(vsUISelectionType.vsUISelectionTypeSelect);
    }

    private void DebugStartNewInstance()
    {
        _dte.ExecuteCommand("ClassViewContextMenus.ClassViewProject.Debug.StartNewInstance");
    }

    private Project FindProjectByName(string projectName)
    {
        foreach (Project project in _dte.Solution.Projects)
        {
            if (project.Name == projectName)
            {
                return project;
            }
        }

        return null;
    }
}
