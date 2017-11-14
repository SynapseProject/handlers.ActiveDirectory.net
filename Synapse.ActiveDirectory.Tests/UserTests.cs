using System;
using System.DirectoryServices.AccountManagement;
using NUnit.Framework;
using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests
{
    /// <summary>
    /// Summary description for UserTests
    /// </summary>
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void GetUser()
        {
            UserPrincipalObject upo = DirectoryServices.GetUser( "wagug0", false, false, false );
        }

        //[Test]
        //public void DisableUserAccount_Without_Username_Throw_Exception()
        //{
        //    // Arrange
        //    string username = "";

        //    // Act
        //    Exception ex = Assert.Throws<AdException>(() => DirectoryServices.DisableUserAccount(username));

        //    // Assert
        //    Assert.That(ex.Message, Is.EqualTo("Username is not provided."));
        //}

    }
}
