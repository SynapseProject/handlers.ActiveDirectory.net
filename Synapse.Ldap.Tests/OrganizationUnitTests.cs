﻿using System;
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
        public void CreateOrganizationUnit_Without_New_Organization_Unit_Throw_Exception()
        {
            // Arrange 
            string parentOrgUnitDistName = "";
            string newOrgUnitName = "";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateOrganizationUnit(parentOrgUnitDistName, newOrgUnitName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("New organization unit is not specified."));
        }

        [Test]
        public void CreateOrganizationUnit_Without_Parent_Organization_Unit_Default_To_Child_Under_Root()
        {
            // Arrange 
            string parentOrgUnitDistName = "";
            string newOrgUnitName = $"TestOU-{DirectoryServices.GenerateToken(8)}";
            string newOrgUnitDn = $"OU={newOrgUnitName},{DirectoryServices.GetDomainDistinguishedName()}";

            // Act
            DirectoryServices.CreateOrganizationUnit(parentOrgUnitDistName, newOrgUnitName);

            // Assert
            Assert.IsTrue(DirectoryServices.IsExistingOrganizationUnit(newOrgUnitDn));
        }

        [Test]
        public void CreateOrganizationUnit_Non_Existing_Parent_Organization_Unit_Throw_Exception()
        {
            // Arrange 
            string parentOrgUnitDistName = "OU=XXX";
            string newOrgUnitName = $"TestOU-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateOrganizationUnit(parentOrgUnitDistName, newOrgUnitName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Parent organization unit does not exist."));
        }

        [Test]
        public void CreateOrganizationUnit_New_Organization_Unit_Already_Exist_Throw_Exception()
        {
            // Arrange 
            string parentOrgUnitDistName = "";
            string newOrgUnitName = $"TestOU-{DirectoryServices.GenerateToken(8)}";

            // Act
            DirectoryServices.CreateOrganizationUnit(parentOrgUnitDistName, newOrgUnitName);
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateOrganizationUnit(parentOrgUnitDistName, newOrgUnitName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("New organization unit already exists."));
        }

        [Test]
        public void CreateOrganizationUnit_With_Valid_Details_Succeed()
        {
            // Arrange 
            string parentOrgUnitDistName = DirectoryServices.GetDomainDistinguishedName();
            string newOrgUnitName = $"TestOU-{DirectoryServices.GenerateToken(8)}";
            string newOrgUnitPath = $"OU={newOrgUnitName},{parentOrgUnitDistName}";

            // Act
            DirectoryServices.CreateOrganizationUnit(parentOrgUnitDistName, newOrgUnitName);

            // Assert
            Assert.IsTrue(DirectoryServices.IsExistingOrganizationUnit(newOrgUnitPath));
        }

        [Test]
        public void CreateOrganizationUnit_With_Valid_Details_Dry_Run_Wont_Save()
        {
            // Arrange 
            string parentOrgUnitDistName = DirectoryServices.GetDomainDistinguishedName();
            string newOrgUnitName = $"TestOU-{DirectoryServices.GenerateToken(8)}";
            string newOrgUnitPath = $"OU={newOrgUnitName},{parentOrgUnitDistName}";

            // Act
            DirectoryServices.CreateOrganizationUnit(parentOrgUnitDistName, newOrgUnitName, "", true);

            // Assert
            Assert.IsFalse(DirectoryServices.IsExistingOrganizationUnit(newOrgUnitPath));
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

            var domainRoot = DirectoryServices.GetDomainDistinguishedName();
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
