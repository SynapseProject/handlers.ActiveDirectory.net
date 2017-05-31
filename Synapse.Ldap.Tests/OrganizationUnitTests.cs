using System;
using System.Runtime.Remoting;
using NUnit.Framework;
using Synapse.Ldap.Core;

namespace Synapse.Ldap.Tests
{
    [TestFixture]
    public class OrganizationUnitTests
    {
        [Test]
        public void CreateOrgUnitWithInvalidParentReturnFailure()
        {
            Assert.Throws<Exception>(() => DirectoryServices.CreateOrganizationUnit("XXXX", "testSubOU"));
        }

        [Test]
        public void CreateOrgUnitWithEmptyParentReturnSuccess()
        {
            var testOrgUnit = DirectoryServices.CreateOrganizationUnit("", $"OU-{Guid.NewGuid()}");
            Assert.IsNotNull(testOrgUnit);
        }

        [Test]
        public void CreateOrgUnitWithValidParentReturnSuccess()
        {
            var testOrgUnit = DirectoryServices.CreateOrganizationUnit("OU=TestOU,DC=bp1,DC=local", $"OU-{Guid.NewGuid()}");
            Assert.IsNotNull(testOrgUnit);
        }
    }
}
