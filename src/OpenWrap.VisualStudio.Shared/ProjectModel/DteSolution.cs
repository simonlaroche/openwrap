using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using OpenFileSystem.IO;
using OpenWrap.ProjectModel;
using OpenWrap.VisualStudio.SolutionAddIn;

namespace OpenWrap.VisualStudio.ProjectModel
{
    using EnvDTE80;

    public class DteSolution : ISolution, IDisposable
    {
        Solution _solution;
        List<DteProject> _projects;
        bool _disposed;
        SolutionEvents _solutionEvents;

        public event EventHandler<EventArgs> ProjectChanged = (src,ea)=> { };

        public DteSolution(Solution solution)
        {
            _solution = solution;
            _projects = GetProjects( _solution).Select(x => new DteProject(x)).ToList();
            _solutionEvents = _solution.DTE.Events.SolutionEvents;
            _solutionEvents.ProjectAdded += HandleProjectAdded;
            _solutionEvents.ProjectRemoved += HandleProjectRemoved;
            _solutionEvents.ProjectRenamed += HandleProjectRenamed;
            Version = new Version(solution.DTE.Version);
        }

        static IEnumerable<Project> GetProjects(EnvDTE.Solution solution)
        {
            var projects = solution.Projects;
            var list = new List<Project>();
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                var project = item.Current as Project;
                if (project == null)
                {
                    continue;
                }

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    list.Add(project);
                }
            }

            return list;
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            var list = new List<Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                {
                    continue;
                }

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                list.Add(subProject);
                }
            }

            return list;
        }


        void HandleProjectRenamed(Project project, string oldname)
        {
            if (_disposed) return;
            ProjectChanged.Raise(this, EventArgs.Empty);
        }

        void HandleProjectRemoved(Project project)
        {
            if (_disposed) return;

            lock (_projects)
                _projects.RemoveAll(x => x.DteObject == project);
            ProjectChanged.Raise(this, EventArgs.Empty);

        }

        void HandleProjectAdded(Project project)
        {
            if (_disposed) return;

            lock(_projects)
                _projects.Add(new DteProject(project));
            ProjectChanged.Raise(this, EventArgs.Empty);
        }

        public IEnumerable<IProject> AllProjects
        {
            get
            {
                CheckAccess();
                lock (_projects)
                {
                    return _projects.Cast<IProject>().ToList();
                }
            }
        }

        public Version Version { get; private set; }

        public bool OpenWrapAddInEnabled
        {
            get
            {
                CheckAccess();
                _solution.AddIns.Update();
                return _solution.AddIns.OfType<AddIn>().Any(x => x.ProgID == ComConstants.ADD_IN_PROGID_2012 || x.ProgID == ComConstants.ADD_IN_PROGID_2010 || x.ProgID == ComConstants.ADD_IN_PROGID_2008);
            }
            set
            {
                CheckAccess();
                if (value && !OpenWrapAddInEnabled)
                {
                    if (_solution.DTE.Version == "9.0")
                        _solution.AddIns.Add(ComConstants.ADD_IN_PROGID_2008, ComConstants.ADD_IN_DESCRIPTION, ComConstants.ADD_IN_NAME, true);
                    else if (_solution.DTE.Version == "10.0")
                        _solution.AddIns.Add(ComConstants.ADD_IN_PROGID_2010, ComConstants.ADD_IN_DESCRIPTION, ComConstants.ADD_IN_NAME, true);
                    else if (_solution.DTE.Version == "11.0")
                        _solution.AddIns.Add(ComConstants.ADD_IN_PROGID_2012, ComConstants.ADD_IN_DESCRIPTION, ComConstants.ADD_IN_NAME, true);
                }
                else if (value == false && OpenWrapAddInEnabled)
                {
                    _solution.AddIns.Cast<AddIn>()
                        .Where(x => x.ProgID == ComConstants.ADD_IN_PROGID_2008 || x.ProgID == ComConstants.ADD_IN_PROGID_2010 || x.ProgID == ComConstants.ADD_IN_PROGID_2012)
                        .ToList().ForEach(x => x.Remove());
                }
            }
        }

        public void Save()
        {
            CheckAccess();
            if (!_solution.IsDirty) return;
            _solution.DTE.ExecuteCommand("File.SaveAll");
            while (_solution.IsDirty)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public IProject AddProject(IFile projectFile)
        {
            CheckAccess();
            return new DteProject(_solution.AddFromFile(projectFile.Path));
        }
        void CheckAccess()
        {
            if (_disposed)
                throw new ObjectDisposedException("The DteSolution instance has been disposed.");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                _solutionEvents.ProjectAdded -= HandleProjectAdded;
                _solutionEvents.ProjectRemoved -= HandleProjectRemoved;
                _solutionEvents.ProjectRenamed -= HandleProjectRenamed;
            }catch{}
            _solutionEvents = null;
            _projects = null;

            _solution = null;
        }
    }
}