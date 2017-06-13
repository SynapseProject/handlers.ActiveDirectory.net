using System;
using System.DirectoryServices.AccountManagement;
using NUnit.Framework;
using Synapse.Ldap.Core;

namespace Synapse.Ldap.Tests
{
    [TestFixture]
    public class GroupTests
    {
        [Test]
        public void CreateGroupEx_Without_OU_Path_Throw_Exception()
        {
            // Arrange 
            string ouPath = "";
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}";

            // Act

            // Assert
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateGroupEx(ouPath, groupName));
            Assert.That(ex.Message, Is.EqualTo("OU path is not specified."));
        }

        [Test]
        public void CreateGroupEx_Without_Group_Name_Throw_Exception()
        {
            // Arrange 
            string ouPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string groupName = "";

            // Act

            // Assert
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateGroupEx(ouPath, groupName));
            Assert.That(ex.Message, Is.EqualTo("Group name is not specified."));
        }

        [Test]
        public void CreateGroupEx_Existing_Group_Throw_Exception()
        {
            // Arrange 
            string ouPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string groupName = "TestGroup1";

            // Act

            // Assert
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateGroupEx(ouPath, groupName));
            Assert.That(ex.Message, Is.EqualTo("The group already exists."));
        }

        [Test]
        public void CreateGroupEx_In_Invalid_OU_Throw_Exception()
        {
            // Arrange 
            string ouPath = "XXX";
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}";

            // Act

            // Assert
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateGroupEx(ouPath, groupName));
            Assert.That(ex.Message, Is.EqualTo("Unable to connect to the domain controller. Check the OU path."));
        }

        [Test]
        public void CreateGroupEx_Create_Universal_Security_Group_By_Default()
        {
            // Arrange 
            string ouPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}";

            // Act
            Console.WriteLine($"Creating universal security group {groupName} under {ouPath}...");
            GroupPrincipal gp = DirectoryServices.CreateGroupEx(ouPath, groupName);

            // Assert
            Assert.That(gp.GroupScope, Is.EqualTo(GroupScope.Universal));
            Assert.That(gp.IsSecurityGroup, Is.True);
            Assert.IsNotNull(gp.Guid);
        }

        [Test]
        public void CreateGroupEx_Create_Universal_Security_Group_DryRun()
        {
            // Arrange 
            string ouPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}";

            // Act
            Console.WriteLine($"Simulating creation of universal security group {groupName} under {ouPath}...");
            GroupPrincipal gp = DirectoryServices.CreateGroupEx(ouPath, groupName, null, GroupScope.Universal, true, true);

            // Assert
            Assert.IsNull(gp.Guid);
        }

        [Test]
        public void DeleteGroupEx_Delete_NonExistent_Group_Throw_Exception()
        {
            // Arrange 
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.DeleteGroupEx(groupName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Group does not exist."));
        }

        [Test]
        public void DeleteGroupEx_Delete_Existing_Group_Succeed()
        {
            // Arrange 
            string ouPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}";
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateGroupEx(ouPath, groupName, description);

            // Assert
            Assert.DoesNotThrow(() => DirectoryServices.DeleteGroupEx(groupName));
        }

        [Test]
        public void DeleteGroupEx_Delete_NonExistent_Group_DryRun_Throw_Exception()
        {
            // Arrange 
            string ouPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}";

            // Act
            Console.WriteLine($"Simulating deletion of group {groupName}...");
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.DeleteGroupEx(groupName, true));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Group does not exist."));
        }

        [Test]
        public void AddUserToGroupEx_Non_Existent_User_Throw_Exception()
        {
            // Arrange 
            string username = $"User-{DirectoryServices.GenerateToken(8)}";
            string groupName = "TestGroup1";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.AddUserToGroupEx(username, groupName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void AddUserToGroupEx_Already_Member_Throw_Exception()
        {
            // Arrange 
            string username = "johndoe";
            string groupName = "TestGroup1";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.AddUserToGroupEx(username, groupName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User already exists in the group."));
        }

        [Test]
        public void AddUserToGroupEx_Already_Member_DryRun_Throw_Exception()
        {
            // Arrange 
            string username = "johndoe";
            string groupName = "TestGroup1";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.AddUserToGroupEx(username, groupName, true));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User already exists in the group."));
        }

        [Test]
        public void AddUserToGroupEx_Not_Yet_A_Member_Succeed()
        {
            // Arrange 
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string givenName = "TestUser";
            string surname = "Synapse";
            string groupName = "TestGroup1"; // This group should always exist.
            string password = "1x034abe5A#1!";
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser("", username, password, givenName, surname, description);
            DirectoryServices.AddUserToGroupEx(username, groupName, false);

            // Assert
            Assert.IsTrue(DirectoryServices.IsUserGroupMember(username, groupName));
        }

        [Test]
        public void RemoveUserFromGroupEx_Non_Existent_User_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string givenName = "TestUser";
            string surname = "Synapse";
            string groupName = "TestGroup1"; // This group should always exist.
            string password = "1x034abe5A#1!";
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser("", username, password, givenName, surname, description);
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.RemoveUserFromGroupEx(username, groupName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void RemoveUserFromGroupEx_Non_Existent_Group_Throw_Exception()
        {
            // Arrange
            string username = "johndoe";
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}"; // This group should always exist.


            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.RemoveUserFromGroupEx(username, groupName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Group cannot be found."));
        }

        [Test]
        public void RemoveUserFromGroupEx_Not_A_Member_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string givenName = "TestUser";
            string surname = "Synapse";
            string groupName = "TestGroup1"; // This group should always exist.
            string password = "1x034abe5A#1!";
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser("", username, password, givenName, surname, description);
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.RemoveUserFromGroupEx(username, groupName));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User does not exist in the group."));
        }

        [Test]
        public void RemoveUserFromGroupEx_Not_A_Member_DryRun_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string givenName = "TestUser";
            string surname = "Synapse";
            string groupName = "TestGroup1"; // This group should always exist.
            string password = "1x034abe5A#1!";
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser("", username, password, givenName, surname, description);
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.RemoveUserFromGroupEx(username, groupName, true));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User does not exist in the group."));
        }

        [Test]
        public void RemoveUserFromGroupEx_Is_A_Member_Succeed()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string givenName = "TestUser";
            string surname = "Synapse";
            string groupName = "TestGroup1"; // This group should always exist.
            string password = "1x034abe5A#1!";
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser("", username, password, givenName, surname, description);
            DirectoryServices.AddUserToGroupEx(username, groupName);
            DirectoryServices.RemoveUserFromGroupEx(username, groupName);

            // Assert
            Assert.IsFalse(DirectoryServices.IsUserGroupMember(username, groupName));
        }

        [Test]
        public void UpdateGroupAttribute_Invalid_Group_Throw_Exception()
        {
            // Arrange
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}";
            string attribute = "description";
            string value = "";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.UpdateGroupAttribute(groupName, attribute, value));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Group does not exist."));
        }

        [Test]
        public void UpdateGroupAttribute_Non_Supported_Attribute_Throw_Exception()
        {
            // Arrange
            string groupName = "TestGroup1"; // Assuming this group always exist.
            string attribute = "XXXXX";
            string value = "";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.UpdateGroupAttribute(groupName, attribute, value));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("The attribute is not supported."));
        }

        [Test]
        public void UpdateGroupAttribute_Non_Supported_Attribute_DryRun_Throw_Exception()
        {
            // Arrange
            string groupName = "TestGroup1"; // Assuming this group always exist.
            string attribute = "XXXXX";
            string value = "";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.UpdateGroupAttribute(groupName, attribute, value, true));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("The attribute is not supported."));
        }

        [Test]
        public void UpdateGroupAttribute_With_Valid_Group_Display_Name_Succeed()
        {
            // Arrange
            string groupName = "TestGroup1"; // Assuming this group always exist.
            string attribute = "displayName";
            string value = "TestGroup1";

            // Act
            DirectoryServices.UpdateGroupAttribute(groupName, attribute, value);
            GroupPrincipal gp = DirectoryServices.GetGroup(groupName);

            // Assert
            Assert.That(gp.DisplayName, Is.EqualTo(value));
        }

        [Test]
        public void UpdateGroupAttribute_With_Blank_Group_Description_Succeed()
        {
            // Arrange
            string groupName = "TestGroup1"; // Assuming this group always exist.
            string attribute = "description";
            string value = "";

            // Act
            DirectoryServices.UpdateGroupAttribute(groupName, attribute, value);
            GroupPrincipal gp = DirectoryServices.GetGroup(groupName);

            // Assert
            Assert.That(gp.Description, Is.Null);
        }
    }
}
