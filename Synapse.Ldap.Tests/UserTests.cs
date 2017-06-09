using System;
using NUnit.Framework;
using Synapse.Ldap.Core;

namespace Synapse.Ldap.Tests
{
    /// <summary>
    /// Summary description for UserTests
    /// </summary>
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void MoveUserToOrganizationUnitSucceeds()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
            
            string destOrgUnit = $"OU=TestOU,{DirectoryServices.GetDomainName()}";

            // Act
            Console.WriteLine("Moving user to destination OU...");
            bool status = DirectoryServices.MoveUserToOrganizationUnit($"CN={userName},{DirectoryServices.GetDomainName()}", destOrgUnit);

            // Assert
            Assert.IsTrue(status);
        }


        [Test]
        public void MoveUserToOrganizationUnitWithInvalidOrganizationUnitFails()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
            string destOrgUnit = $"OU=XXXX,{DirectoryServices.GetDomainName()}";

            // Act
            Console.WriteLine("Moving user to destination OU...");
            bool status = DirectoryServices.MoveUserToOrganizationUnit($"CN={userName},{DirectoryServices.GetDomainName()}", destOrgUnit);

            // Assert
            Assert.IsFalse(status);
        }

        [Test]
        public void CreateUserReturnGuid()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";

            // Act
            // Assert
            Assert.DoesNotThrow(() => DirectoryServices.CreateUser(ldapPath, userName, userPassword));
        }

        [Test]
        public void AddUserToGroupReturnSuccess()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
            string groupDn = $"CN=TestGroup,{ldapPath}";

            // Act
            bool status = DirectoryServices.AddUserToGroup($"CN={userName},{ldapPath}", groupDn);

            // Assert
            Assert.IsTrue(status);
        }

        [Test]
        public void AddUserToGroupWithInvalidGroupReturnFailure()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
            string groupDn = $"CN=XXXXXX,{ldapPath}";

            // Act
            bool status = DirectoryServices.AddUserToGroup($"CN={userName},{ldapPath}", groupDn);

            // Assert
            Assert.IsFalse(status);
        }

        [Test]
        public void RemoveUserFromGroupReturnSuccess()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
            string groupDn = $"CN=TestGroup,{ldapPath}";

            // Act
            bool status = DirectoryServices.RemoveUserFromGroup($"CN={userName},{ldapPath}", groupDn);

            // Assert
            Assert.IsTrue(status);
        }

        [Test]
        public void RemoveUserFromGroupWithInvalidGroupReturnFailure()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
            string groupDn = $"CN=XXXXXX,{ldapPath}";

            // Act
            bool status = DirectoryServices.RemoveUserFromGroup($"CN={userName},{ldapPath}", groupDn);

            // Assert
            Assert.IsFalse(status);
        }

        [Test]
        public void DeleteUserReturnSuccess()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);

            // Act
            bool status = DirectoryServices.DeleteUser(userName);

            // Assert
            Assert.IsTrue(status);
        }

        [Test]
        public void DeleteUserWithInvalidDetailReturnFailure()
        {
            // Arrange 

            // Act
            bool status = DirectoryServices.DeleteUser("XXXXXXX");

            // Assert
            Assert.IsFalse(status);
        }

        [Test]
        public void UpdateUserInfoReturnSuccess()
        {
            // Arrange 
            string domain = DirectoryServices.GetDomainName();
            // Act
            bool status = DirectoryServices.UpdateUserAttribute("User-xLPO5dgma3U", DirectoryServices.Property.description, "Updated by Synapse ADAPI", domain);

            // Assert
            Assert.IsTrue(status);
        }
    }
}
