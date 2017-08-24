using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Remoting;
using NUnit.Framework;
using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests
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
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.CreateOrganizationUnit( newOrgUnitName, parentOrgUnitDistName, ""));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("New organization unit is not specified."));
        }

        [Test]
        public void CreateOrganizationUnit_Without_Parent_Organization_Unit_Default_To_Child_Under_Root()
        {
            // Arrange 
            string parentOrgUnitDistName = "";
            string newOrgUnitName = $"TestOU-{Utility.GenerateToken(8)}";
            string newOrgUnitDn = $"OU={newOrgUnitName},{DirectoryServices.GetDomainDistinguishedName()}";

            // Act
            DirectoryServices.CreateOrganizationUnit( newOrgUnitName, parentOrgUnitDistName, "");

            // Assert
            Assert.IsTrue(DirectoryServices.IsExistingOrganizationUnit(newOrgUnitDn));
        }

        [Test]
        public void CreateOrganizationUnit_Non_Existing_Parent_Organization_Unit_Throw_Exception()
        {
            // Arrange 
            string parentOrgUnitDistName = $"OU=xxxxxx,{DirectoryServices.GetDomainDistinguishedName()}";
            string newOrgUnitName = $"TestOU-{Utility.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.CreateOrganizationUnit(newOrgUnitName, parentOrgUnitDistName, ""));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Parent organization unit does not exist."));
        }

        [Test]
        public void CreateOrganizationUnit_New_Organization_Unit_Already_Exist_Throw_Exception()
        {
            // Arrange 
            string parentOrgUnitDistName = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string newOrgUnitName = $"TestOU-{Utility.GenerateToken(8)}";

            // Act
            DirectoryServices.CreateOrganizationUnit( newOrgUnitName, parentOrgUnitDistName, "" );
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.CreateOrganizationUnit(newOrgUnitName, parentOrgUnitDistName, "") );

            // Assert
            Assert.That(ex.Message, Is.EqualTo("New organization unit already exists."));
        }

        [Test]
        public void CreateOrganizationUnit_With_Valid_Details_Succeed()
        {
            // Arrange 
            string parentOrgUnitDistName = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string newOrgUnitName = $"TestOU-{Utility.GenerateToken(8)}";
            string newOrgUnitPath = $"OU={newOrgUnitName},{parentOrgUnitDistName}";

            // Act
            DirectoryServices.CreateOrganizationUnit( newOrgUnitName, parentOrgUnitDistName, "");

            // Assert
            Assert.IsTrue(DirectoryServices.IsExistingOrganizationUnit(newOrgUnitPath));
        }

        [Test]
        public void CreateOrganizationUnit_With_Valid_Details_Dry_Run_Wont_Save()
        {
            // Arrange 
            string parentOrgUnitDistName = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string newOrgUnitName = $"TestOU-{Utility.GenerateToken(8)}";
            string newOrgUnitPath = $"OU={newOrgUnitName},{parentOrgUnitDistName}";

            // Act
            DirectoryServices.CreateOrganizationUnit( newOrgUnitName, parentOrgUnitDistName, "", true );

            // Assert
            Assert.IsFalse(DirectoryServices.IsExistingOrganizationUnit(newOrgUnitPath));
        }

        [Test]
        public void DeleteOrganizationUnit_Without_Organization_Unit_Specified_Throw_Exception()
        {
            // Arrange 
            string orgUnitDistName = "";

            // Act
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.DeleteOrganizationUnit(orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Organization unit is not specified."));
        }

        [Test]
        public void DeleteOrganizationUnit_With_Non_Existent_Organization_Unit_Throw_Exception()
        {
            // Arrange 
            string orgUnitDistName = "OU=XXX";

            // Act
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.DeleteOrganizationUnit(orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Organization unit cannot be found."));
        }

        [Test]
        public void DeleteOrganizationUnit_Existing_Organization_Unit_Succeed()
        {
            // Arrange 
            string parentOrgUnitDistName = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string newOrgUnitName = $"TestOU-{Utility.GenerateToken(8)}";
            string newOrgUnitPath = $"OU={newOrgUnitName},{parentOrgUnitDistName}";

            // Act
            DirectoryServices.CreateOrganizationUnit(newOrgUnitName, parentOrgUnitDistName, "");
            DirectoryServices.DeleteOrganizationUnit(newOrgUnitPath);

            // Assert
            Assert.IsFalse(DirectoryServices.IsExistingOrganizationUnit(newOrgUnitPath));
        }

        [Test]
        public void DeleteOrganizationUnit_Existing_Organization_Unit_Dry_Run_Wont_Delete()
        {
            // Arrange 
            string parentOrgUnitDistName = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string newOrgUnitName = $"TestOU-{Utility.GenerateToken(8)}";
            string newOrgUnitPath = $"OU={newOrgUnitName},{parentOrgUnitDistName}";

            // Act
            DirectoryServices.CreateOrganizationUnit( newOrgUnitName, parentOrgUnitDistName, "" );
            DirectoryServices.DeleteOrganizationUnit(newOrgUnitPath, true);

            // Assert
            Assert.IsTrue(DirectoryServices.IsExistingOrganizationUnit(newOrgUnitPath));
        }

        [Test]
        public void MoveGroupToOrganizationUnit_Without_Group_Name_Throw_Exception()
        {
            // Arrange 
            string groupName = "";
            string orgUnitDistName = "XXX";

            // Act
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.MoveGroupToOrganizationUnit(groupName, orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Group is not specified."));
        }

        [Test]
        public void MoveGroupToOrganizationUnit_Without_Organization_Unit_Throw_Exception()
        {
            // Arrange 
            string groupName = $"TestGroup-{Utility.GenerateToken(8)}";
            string ldapPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string orgUnitDistName = "";

            // Act
            DirectoryServices.CreateGroup(groupName, ldapPath, "");
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.MoveGroupToOrganizationUnit(groupName, orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Organization unit is not specified."));
        }

        [Test]
        public void MoveGroupToOrganizationUnit_Non_Existent_Group_Throw_Exception()
        {
            // Arrange 
            string username = $"TestGroup-{Utility.GenerateToken(8)}";
            string orgUnitDistName = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";

            // Act
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.MoveGroupToOrganizationUnit(username, orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Group cannot be found."));
        }

        [Test]
        public void MoveGroupToOrganizationUnit_Non_Existent_Organization_Unit_Throw_Exception()
        {
            // Arrange 
            string groupName = $"TestGroup-{Utility.GenerateToken(8)}";
            string ldapPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string orgUnitDistName = "XXX";

            // Act
            DirectoryServices.CreateGroup(groupName, ldapPath, "");
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.MoveGroupToOrganizationUnit(groupName, orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Organization unit cannot be found."));
        }

        [Test]
        public void MoveGroupToOrganizationUnit_With_Valid_Details_Succeed()
        {
            // Arrange 
            string groupName = $"TestGroup-{Utility.GenerateToken(8)}";
            string ldapPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";

            string orgUnitName = $"TestOU-{Utility.GenerateToken(8)}";
            string orgUnitDistName = $"OU={orgUnitName},{ldapPath}";

            // Act
            DirectoryServices.CreateGroup(groupName, ldapPath, "");
            DirectoryServices.CreateOrganizationUnit(orgUnitName, ldapPath, "");
            DirectoryServices.MoveGroupToOrganizationUnit(groupName, orgUnitDistName);

            // Assert
            Assert.That(orgUnitName, Is.EqualTo(Utility.GetGroupOrganizationUnit(groupName)));
        }


        [Test]
        public void MoveGroupToOrganizationUnit_With_Valid_Details_Dry_Run_Not_A_Member()
        {
            // Arrange 
            string groupName = $"TestGroup-{Utility.GenerateToken(8)}";
            string ldapPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";

            string orgUnitName = $"TestOU-{Utility.GenerateToken(8)}";
            string orgUnitDistName = $"OU={orgUnitName},{ldapPath}";

            // Act
            DirectoryServices.CreateGroup(groupName, ldapPath, "");
            DirectoryServices.CreateOrganizationUnit(orgUnitName, ldapPath, "");
            DirectoryServices.MoveGroupToOrganizationUnit(groupName, orgUnitDistName, true);

            // Assert
            Assert.AreNotEqual(orgUnitName, Utility.GetUserOrganizationUnit(groupName));
        }


        [Test]
        public void MoveUserToOrganizationUnit_Without_Username_Throw_Exception()
        {
            // Arrange 
            string username = "";
            string orgUnitDistName = "XXX";

            // Act
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.MoveUserToOrganizationUnit(username, orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User is not specified."));
        }

        [Test]
        public void MoveUserToOrganizationUnit_Without_Organization_Unit_Throw_Exception()
        {
            // Arrange 
            string username = $"TestUser{Utility.GenerateToken(8)}";
            string ldapPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";
            string orgUnitDistName = "";

            // Act
            DirectoryServices.CreateUser(username, ldapPath, userPassword, givenName, surname, description);
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.MoveUserToOrganizationUnit(username, orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Organization unit is not specified."));
        }

        [Test]
        public void MoveUserToOrganizationUnit_Non_Existent_User_Throw_Exception()
        {
            // Arrange 
            string username = $"TestUser{Utility.GenerateToken(8)}";
            string orgUnitDistName = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";

            // Act
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.MoveUserToOrganizationUnit(username, orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void MoveUserToOrganizationUnit_Non_Existent_Organization_Unit_Throw_Exception()
        {
            // Arrange 
            string username = $"TestUser{Utility.GenerateToken(8)}";
            string ldapPath = "";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";
            string orgUnitDistName = "XXX";

            // Act
            DirectoryServices.CreateUser(username, ldapPath, userPassword, givenName, surname, description);
            Exception ex = Assert.Throws<AdException>(() => DirectoryServices.MoveUserToOrganizationUnit(username, orgUnitDistName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Organization unit cannot be found."));
        }

        [Test]
        public void MoveUserToOrganizationUnit_With_Valid_Details_Succeed()
        {
            // Arrange 
            string username = $"TestUser{Utility.GenerateToken(8)}";
            string ldapPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";
            string orgUnitName = $"TestOU-{Utility.GenerateToken(8)}";
            string orgUnitDistName = $"OU={orgUnitName},{ldapPath}";

            // Act
            DirectoryServices.CreateUser(username, ldapPath, userPassword, givenName, surname, description);
            DirectoryServices.CreateOrganizationUnit(orgUnitName, ldapPath, "");
            DirectoryServices.MoveUserToOrganizationUnit(username, orgUnitDistName);

            // Assert
            Assert.That(orgUnitName, Is.EqualTo( Utility.GetUserOrganizationUnit(username)));
        }

        [Test]
        public void MoveUserToOrganizationUnit_With_Valid_Details_Dry_Run_Not_A_Member()
        {
            // Arrange 
            string username = $"TestUser{Utility.GenerateToken(8)}";
            string ldapPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";
            string orgUnitName = $"TestOU-{Utility.GenerateToken(8)}";
            string orgUnitDistName = $"OU={orgUnitName},{ldapPath}";

            // Act
            DirectoryServices.CreateUser(username, ldapPath, userPassword, givenName, surname, description);
            DirectoryServices.CreateOrganizationUnit( orgUnitName, ldapPath, "" );
            DirectoryServices.MoveUserToOrganizationUnit(username, orgUnitDistName, true);

            // Assert
            Assert.AreNotEqual(orgUnitName, Utility.GetUserOrganizationUnit(username));
        }
    }
}
