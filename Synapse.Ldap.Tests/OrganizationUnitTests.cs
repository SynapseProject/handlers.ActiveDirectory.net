using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
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
            var testOrgUnit = DirectoryServices.CreateOrganizationUnit("", $"OU-{DirectoryServices.GenerateToken(8)}");
            Assert.IsNotNull(testOrgUnit);
        }

        [Test]
        public void CreateOrgUnitWithValidParentReturnSuccess()
        {
            var testOrgUnit = DirectoryServices.CreateOrganizationUnit("OU=TestOU,DC=bp1,DC=local", $"OU-{DirectoryServices.GenerateToken(8)}");
            Assert.IsNotNull(testOrgUnit);
        }

        [Test]
        public void DeleteOrgUnitWithInvalidNameReturnFalse()
        {
            Assert.IsFalse(DirectoryServices.DeleteOrganizationUnit("testSubOU"));
        }


        [Test]
        public void DeleteOrgUnitWithValidNameReturnTrue()
        {
            var ouName = $"OU-{DirectoryServices.GenerateToken(8)}";
            Console.WriteLine($"Creating OU - {ouName}...");
            DirectoryServices.CreateOrganizationUnit("", ouName);

            var domainRoot = DirectoryServices.GetDomainName();
            Console.WriteLine($"Deleting OU - {ouName}...");

            Assert.IsTrue(DirectoryServices.DeleteOrganizationUnit($"OU={ouName},{domainRoot}"));
        }

        [Test]
        public void FriendlyDomainToLdapDomainReturnExpectedResult()
        {
            var fqdn = DirectoryServices.FriendlyDomainToLdapDomain("bp1");
            Assert.AreEqual(fqdn, "bp1.local");
        }

        [Test]
        public void FriendlyDomainToLdapDomainWithInvalidNameReturnException()
        {
            Assert.Throws<ActiveDirectoryObjectNotFoundException>(() => DirectoryServices.FriendlyDomainToLdapDomain("XXX"));
        }


        [Test]
        public void EnumerateOUMembersReturnAtLeastOneRecord()
        {
            List<string> members = DirectoryServices.EnumerateOUMembers("DC=bp1, DC=local");

            Assert.That(members.Count, Is.GreaterThan(0));
        }
    }
}
