extern alias resharper;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using EnvironmentComponent = resharper::JetBrains.Application.Env.EnvironmentComponentAttribute;
using IPluginSource = resharper::JetBrains.Application.PluginSupport.IPluginSource;
using NotNull = resharper::JetBrains.Annotations.NotNullAttribute;
using FileSystemPath = resharper::JetBrains.Util.FileSystemPath;
using PlugInSourceInfo = resharper::JetBrains.Application.PluginSupport.PluginSourceInfo;

namespace OpenWrap.Resharper
{
    

  [EnvironmentComponent(resharper::JetBrains.Application.Sharing.Product)]
  public class CollectPluginsOpenWrapCacheFolder :  IPluginSource
  {
      readonly string _location;
      public static readonly string InfoRecordSenderId = "OpenwrapPluginFolders";
      resharper::JetBrains.DataFlow.Lifetime _lifetime;
      resharper::JetBrains.Application.IApplicationDescriptor _product;


      public CollectPluginsOpenWrapCacheFolder(resharper::JetBrains.DataFlow.Lifetime lifetime, resharper::JetBrains.Application.IApplicationDescriptor product)
      {
          _location = GetType().Assembly.Location;
          _lifetime = lifetime;
          _product = product;
      }

      static PlugInSourceInfo PluginSourceFromDirectory([NotNull] resharper::JetBrains.DataFlow.Lifetime lifetime, [NotNull] FileSystemPath pathDir, [NotNull] resharper::JetBrains.Application.IApplicationDescriptor product, [NotNull] MetadataLoader loader)
      {
          if (pathDir == (FileSystemPath)null)
              throw new ArgumentNullException("pathDir");
          if (product == null)
              throw new ArgumentNullException("product");
          if (loader == null)
              throw new ArgumentNullException("loader");
          if (lifetime == null)
              throw new ArgumentNullException("lifetime");
          try
          {
              if (!pathDir.ExistsDirectory)
              {
                  return PlugInSourceInfo.FromRecord(new resharper::JetBrains.Application.PluginSupport.PluginsDirectory.Record[1]
                  {
                      new resharper::JetBrains.Application.PluginSupport.PluginsDirectory.Record(resharper::JetBrains.Application.PluginSupport.PluginsDirectory.RecordKind.Info,
                                                                                                 InfoRecordSenderId,
                                                                                                 string.Format(
                                                                                                     "There are no plugins at the standard plugins location {0} because there is no such directory.",
                                                                                                     (object)resharper::JetBrains.Util.FileSystemPathEx.QuoteIfNeeded(pathDir)),
                                                                                                 (Exception)null)
                  });
              }

              var infoRecords =
                  (ICollection<resharper::JetBrains.Application.PluginSupport.PluginsDirectory.Record>)new List<resharper::JetBrains.Application.PluginSupport.PluginsDirectory.Record>();
              
              var plugins = resharper::JetBrains.Application.PluginSupport.DiscoverPluginsInDirectory
                                            .CreatePluginsFromDirectory(lifetime,
                                                                        pathDir,
                                                                        product,
                                                                        infoRecords,
                                                                        loader);
                 
              infoRecords.Add(new resharper::JetBrains.Application.PluginSupport.PluginsDirectory.Record(resharper::JetBrains.Application.PluginSupport.PluginsDirectory.RecordKind.Info,
                                                                                                         resharper::JetBrains.Application.PluginSupport.CollectPluginsInProductFolders
                                                                                                                  .InfoRecordSenderId,
                                                                                                         string.Format(
                                                                                                             "Collected plugins from {0:N0} plugin folder.",
                                                                                                             pathDir
                                                                                                             )));
              return new PlugInSourceInfo(plugins.ToList(),
                                          infoRecords);
          }
          catch (Exception ex)
          {
              resharper::JetBrains.Util.Logger.LogExceptionSilently(ex);
              return PlugInSourceInfo.FromRecord(new resharper::JetBrains.Application.PluginSupport.PluginsDirectory.Record[1]
              {
                  new resharper::JetBrains.Application.PluginSupport.PluginsDirectory.Record(resharper::JetBrains.Application.PluginSupport.PluginsDirectory.RecordKind.Error,
                                                                                             resharper::JetBrains.Application.PluginSupport.CollectPluginsInProductFolders.InfoRecordSenderId,
                                                                                             (resharper::JetBrains.UI.RichText.RichText)
                                                                                             string.Format("Error collecting plugins from the standard folder {0}. {1}",
                                                                                                           (object)resharper::JetBrains.Util.FileSystemPathEx.QuoteIfNeeded(pathDir),
                                                                                                           (object)resharper::JetBrains.Util.ExceptionRenderer.RenderException(ex).Message),
                                                                                             ex)
              });
          }
      }

      [NotNull]
      public  FileSystemPath[] GetAllPredefinedPluginsFolders()
      {
          return new[]{new FileSystemPath(_location).Directory};
          
      }



      public ICollection<PlugInSourceInfo> DiscoverPluginSourceInfo()
      {
          MetadataLoader loader = new MetadataLoader(new FileSystemPath[0]);
          try
          {
              return GetAllPredefinedPluginsFolders().Select(pathDir => PluginSourceFromDirectory(_lifetime, pathDir, _product, loader)).ToList();
          }
          finally
          {
              if (loader != null)
                  loader.Dispose();
          }
//          var plugin = new resharper::JetBrains.Application.PluginSupport.Plugin(_lifetime, new[] { new resharper::JetBrains.Util.FileSystemPath(_location) }, null, null, null);
//          plugin.IsEnabled.SetValue(true);
//          return
//              new[]{
//              new PlugInSourceInfo(
//              new[]
//              {
//                  plugin
//              },
//              new[]{new resharper::JetBrains.Application.PluginSupport.PluginsDirectory.Record(resharper::JetBrains.Application.PluginSupport.PluginsDirectory.RecordKind.Info, InfoRecordSenderId, (resharper::JetBrains.UI.RichText.RichText) "Loaded OpenWrap Resharper Plugin at " + _location, null), })};
//        return (ICollection<PlugInSourceInfo>) Enumerable.ToList<PlugInSourceInfo>(Enumerable.Select<FileSystemPath, PluginSourceInfo>((IEnumerable<FileSystemPath>) CollectPluginsInProductFolders.GetAllPredefinedPluginsFolders(this.myProductSettingsLocation), (Func<FileSystemPath, PluginSourceInfo>) (pathDir => CollectPluginsInProductFolders.PluginSourceFromDirectory(this.myLifetime, pathDir, this.myProduct, loader))));
//        return (ICollection<PluginSourceInfo>) new PluginSourceInfo[1]
//        {
//          PluginSourceInfo.FromRecord(new PluginsDirectory.Record[1]
//          {
//            new PluginsDirectory.Record(PluginsDirectory.RecordKind.Info, CollectPluginsInProductFolders.InfoRecordSenderId, (RichText) "Loading plugins from standard plugin folders was suppressed by the product.", (Exception) null)
//          })
//        };
      }
  }

}