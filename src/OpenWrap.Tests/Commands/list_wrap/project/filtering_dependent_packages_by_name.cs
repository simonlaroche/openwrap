using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.list_wrap.project
{
    public class filtering_dependent_packages_by_name : command<ListWrapCommand>
    {
        public filtering_dependent_packages_by_name()
        {
            given_project_package("one-ring", "1.0", "depends: sauron");
            given_project_package("sauron", "1.0");

            given_dependency("depends: one-ring");
            when_executing_command("sauron");
        }

        [Test]
        public void dependent_package_is_found()
        {
            Results.OfType<DescriptorPackages>().Single()
                .Packages.Single().Check(_ => _.PackageInfo.Name.ShouldBe("one-ring"))
                .Children.Single().Check(_ => _.PackageInfo.Name.ShouldBe("sauron"))
                .Children.ShouldBe(DescriptorPackages.Truncated);
        }
    }
}