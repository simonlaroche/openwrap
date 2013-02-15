extern alias resharper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using OpenWrap.VisualStudio;

#if v600 || v610 || v710
using ResharperPluginManager = resharper::JetBrains.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginVendorAttribute = resharper::JetBrains.Application.PluginSupport.PluginVendorAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentAttribute;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IProjectToAssemblyReference;
using ResharperThreading = resharper::JetBrains.Threading.IThreading;
#else
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.UI.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperPluginVendorAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginVendorAttribute;
using ResharperThreading = OpenWrap.Resharper.IThreading;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IAssemblyReference;
#endif


[assembly: ResharperPluginTitleAttribute("OpenWrap ReSharper Integration")]
[assembly: ResharperPluginDescription("Provides integration of OpenWrap features within ReSharper.")]
[assembly: ResharperPluginVendorAttribute("Caffeine IT")]
namespace OpenWrap.Resharper
{
    /// <summary>
    /// Provides support for dynamic loading and unloading of ReSharper plugins, always lives in the same AppDomain as ReSharper.
    /// </summary>
    public class PluginManager : MarshalByRefObject, IDisposable
    {
        public const string ACTION_REANALYZE = "ErrorsView.ReanalyzeFilesWithErrors";
        public const string OUTPUT_RESHARPER_TESTS = "OpenWrap-Tests";
        readonly DTE2 _dte;
        List<Assembly> _loadedAssemblies = new List<Assembly>();
        //bool _resharperLoaded;

        static ResharperPlugin _selfPlugin;
        //System.Threading.Thread _debugThread;
        //bool runTestRunner = true;
        OpenWrapOutput _output;
        ResharperThreading _threading;

#if v600 || v610 || v710
        resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost _host;
        resharper::JetBrains.Application.PluginSupport.PluginsDirectory _pluginsDirectory;
        resharper::JetBrains.DataFlow.LifetimeDefinition _lifetimeDefinition;
#endif

        public const string RESHARPER_TEST = "?ReSharper";

        public PluginManager()
        {
            _dte = (DTE2)SiteManager.GetGlobalService<DTE>();
            _output = new OpenWrapOutput("Resharper Plugin Manager");
            _output.Write("Loaded ({0}).", GetType().Assembly.GetName().Version);

#if v600 || v610

            _host = resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost.GetOrCreateHost((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);
           
            var resolvedObj = _host.Environment.Container.ResolveDynamic(typeof(ResharperThreading));
            if (resolvedObj != null)
                _threading = (ResharperThreading)resolvedObj.Instance;
#elif v710
            _host = resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost.GetOrCreateHost((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);
           
            var resolvedObj = _host.Environment.Container.ResolveDynamic(typeof(ResharperThreading));
            if (resolvedObj != null)
                _threading = (ResharperThreading)resolvedObj.Instance;
#else
             _threading = new LegacyShellThreading();
#endif
            if (_threading == null)
            {
                _output.Write("Threading not found, the plugin manager will not initialize.");
                return;
            }
            _threading.Run("Loading plugins...", StartDetection);

        }

        public void Dispose()
        {
            _output.Write("Unloading.");
            //runTestRunner = false;
#if v600 || v610
            _selfPlugin.IsEnabled.SetValue(false);
            _pluginsDirectory.Plugins.Remove(_selfPlugin);
            _lifetimeDefinition.Terminate();
            _host = null;
#elif v710
            
            //_pluginsDirectory.Plugins.Remove(_selfPlugin);
            _lifetimeDefinition.Terminate();
            _host = null;
#else
            _selfPlugin.Enabled = false;
            ResharperPluginManager.Instance.Plugins.Remove(_selfPlugin);
#endif
            _selfPlugin = null;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
        void StartDetection()
        {
            try
            {
                var asm = GetType().Assembly;
#if v600 || v610
                _lifetimeDefinition = resharper::JetBrains.DataFlow.Lifetimes.Define(resharper::JetBrains.DataFlow.EternalLifetime.Instance, "OpenWrap Solution");
                _pluginsDirectory =
                    (resharper::JetBrains.Application.PluginSupport.PluginsDirectory)_host.Environment.Container.ResolveDynamic(typeof(resharper::JetBrains.Application.PluginSupport.PluginsDirectory)).Instance;

                _selfPlugin = new ResharperPlugin(_lifetimeDefinition.Lifetime, new[] { new resharper::JetBrains.Util.FileSystemPath(asm.Location) }, null, null, null);

                _pluginsDirectory.Plugins.Add(_selfPlugin);
                
                _selfPlugin.IsEnabled.SetValue(true);
#elif v710
                _lifetimeDefinition = resharper::JetBrains.DataFlow.Lifetimes.Define(resharper::JetBrains.DataFlow.EternalLifetime.Instance, "OpenWrap Solution");

                //            var asm = GetType().Assembly;
                //            var pluginSource = new CollectPluginsOpenWrapCacheFolder(_lifetimeDefinition.Lifetime);

                var container = new resharper::JetBrains.Application.Components.ComponentContainer(_lifetimeDefinition.Lifetime, "Openwrap container");
                container.Inject<CollectPluginsOpenWrapCacheFolder>();

                _host.Environment.Container.ChainTo(container);
                _output.Write("Injected plugin source in container.");
                var instance = _host.Environment.Container.ResolveDynamic(typeof(IEnumerable<resharper::JetBrains.Application.PluginSupport.IPluginSource>)).Instance;
                _output.Write("Resolving IPlugInSource returns {0}.", instance.GetType());
                var enumemerable = instance as IEnumerable<resharper::JetBrains.Application.PluginSupport   .IPluginSource>;
                if (enumemerable != null)
                {
                    foreach (var source in enumemerable)
                    {
                        _output.Write("IPlugInSource {0}.", source.GetType());
                    }
                }

                var pluginDefinition = (resharper::JetBrains.Application.PluginSupport.IPluginSource)_host.Environment.Container.ResolveDynamic(typeof(CollectPluginsOpenWrapCacheFolder)).Instance;
                var pi =pluginDefinition.DiscoverPluginSourceInfo().First().Plugins.First();
                pi.IsEnabled.SetValue(true);
                _pluginsDirectory =
                    (resharper::JetBrains.Application.PluginSupport.PluginsDirectory)_host.Environment.Container.ResolveDynamic(typeof(resharper::JetBrains.Application.PluginSupport.PluginsDirectory)).Instance;

                var type = typeof(resharper::JetBrains.Application.PluginSupport.PluginsDirectory);
                var fieldInfo = type.GetField("myPlugins", BindingFlags.Instance | BindingFlags.NonPublic);
                var value = (List<ResharperPlugin>)fieldInfo.GetValue(_pluginsDirectory);

                _output.Write("{0} plugin registered.", value.Count());


                value.Add(pi);

//                _output.Write("{0} Plugins in plug in directory", _pluginsDirectory.Plugins.Count());
//
//                    foreach (var plugin in _pluginsDirectory.Plugins)
//                    {
//                        _output.Write("PlugIn {0}.", plugin.AssemblyPaths.First().FullPath);
//                    }

#else
                var id = "ReSharper OpenWrap Integration";
                _selfPlugin = new ResharperPlugin(id, new[] { asm });


                ResharperPluginManager.Instance.Plugins.Add(_selfPlugin);
                _selfPlugin.Enabled = true;
                resharper::JetBrains.Application.Shell.Instance.LoadAssemblies(id, asm);
#endif
            }
            catch (Exception e)
            {
                _output.Write("Plugin integration failed.\r\n" + e.ToString());
            }
        }
    }


}
