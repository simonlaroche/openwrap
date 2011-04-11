﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenWrap.Collections;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;

namespace OpenWrap.Testing
{
    public class TestRunnerManager : ITestRunnerManager
    {
        readonly IEnumerable<ITestRunnerFactory> _factories;
        readonly IEnvironment _environment;
        readonly IPackageManager _manager;

        public TestRunnerManager(IEnumerable<ITestRunnerFactory> factories, IEnvironment environment, IPackageManager manager)
        {
            _factories = factories;
            _environment = environment;
            _manager = manager;
        }

        public IEnumerable<KeyValuePair<string, bool?>> ExecuteAllTests(ExecutionEnvironment environment, IPackage package)
        {
            var descriptor = new PackageDescriptor();
            descriptor.Dependencies.Add(new PackageDependencyBuilder(Guid.NewGuid().ToString()).Name(package.Name).VersionVertex(new EqualVersionVertex(package.Version)));
            throw new NotImplementedException();
            //var allAssemblyReferences = _manager.GetAssemblyReferences(false, descriptor, package.Source, _environment.ProjectRepository, _environment.SystemRepository);

            //var runners = _factories.SelectMany(x=>x.GetTestRunners(allAssemblyReferences)).NotNull();

            //var export = package.GetExport("tests", environment);

            //if (export == null) return Enumerable.Empty<KeyValuePair<string, bool?>>();

            //var testAssemblies = from item in export.Items
            //                     where item.FullPath.EndsWithNoCase(".dll")
            //                     select item.FullPath;
            //return from runner in runners
            //       from asm in testAssemblies
            //       from result in runner.ExecuteTests(allAssemblyReferences.Select(x => x.FullPath).ToList(), testAssemblies)
            //       select result;
        }

    }
}
