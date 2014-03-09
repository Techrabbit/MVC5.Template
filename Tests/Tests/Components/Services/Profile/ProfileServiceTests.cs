﻿using NUnit.Framework;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Template.Components.Alerts;
using Template.Components.Security;
using Template.Components.Services;
using Template.Data.Core;
using Template.Objects;
using Template.Resources.Views.ProfileView;
using Template.Tests.Data;
using Template.Tests.Helpers;
using Tests.Helpers;

namespace Template.Tests.Tests.Components.Services
{
    [TestFixture]
    public class ProfileServiceTests
    {
        private ProfileService service;
        private AContext context;
        private Account account;

        [SetUp]
        public void SetUp()
        {
            context = new TestingContext();
            service = new ProfileService(new UnitOfWork(context));

            service.ModelState = new ModelStateDictionary();
            service.AlertMessages = new MessagesContainer(service.ModelState);
            HttpContext.Current = new HttpContextBaseMock().HttpContext;

            TearDownData();
            SetUpData();
        }

        [TearDown]
        public void TearDown()
        {
            HttpContext.Current = null;

            service.Dispose();
            context.Dispose();
        }

        #region Method: CanEdit(ProfileView profile)

        [Test]
        public void CanEdit_CanNotEditWithInvalidModelState()
        {
            service.ModelState.AddModelError("Key", "ErrorMessages");
            Assert.IsFalse(service.CanEdit(ObjectFactory.CreateProfileView()));
        }

        [Test]
        public void CanEdit_CanEditUsingItsOwnUsername()
        {
            var profile = ObjectFactory.CreateProfileView();
            profile.Username = account.Username.ToUpper();

            Assert.IsTrue(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_CanNotEditToAlreadyTakenUsername()
        {
            var takenAccount = ObjectFactory.CreateAccount();
            takenAccount.User = ObjectFactory.CreateUser();
            takenAccount.User.Id = takenAccount.User.Id + "1";
            takenAccount.UserId = takenAccount.User.Id + "1";
            takenAccount.Username += "1";
            takenAccount.Id += "1";

            context.Set<Account>().Add(takenAccount);
            context.SaveChanges();

            var profile = ObjectFactory.CreateProfileView();
            profile.Username = takenAccount.Username;

            Assert.IsFalse(service.CanEdit(profile));
            Assert.AreEqual(service.ModelState["Username"].Errors[0].ErrorMessage, Validations.UsernameIsAlreadyTaken);
        }

        [Test]
        public void CanEdit_CanNotEditWithIncorrectPassword()
        {
            var profile = ObjectFactory.CreateProfileView();
            profile.CurrentPassword += "1";

            Assert.IsFalse(service.CanEdit(profile));
            Assert.AreEqual(service.ModelState["CurrentPassword"].Errors[0].ErrorMessage, Validations.IncorrectPassword);
        }

        [Test]
        public void CanEdit_CanEditValidProfile()
        {
            Assert.IsTrue(service.CanEdit(ObjectFactory.CreateProfileView()));
        }

        #endregion

        #region Method: CanDelete(ProfileView profile)

        [Test]
        public void CanDelete_CanNotDeleteWithInvalidModelState()
        {
            service.ModelState.AddModelError("Test", "Test");
            Assert.IsFalse(service.CanDelete(ObjectFactory.CreateProfileView()));
        }

        [Test]
        public void CanDelete_CanNotDeleteWithIncorrectUsername()
        {
            var profile = ObjectFactory.CreateProfileView();
            profile.Username = String.Empty;

            Assert.IsFalse(service.CanDelete(profile));
            Assert.AreEqual(service.ModelState["Username"].Errors[0].ErrorMessage, Validations.IncorrectUsername);
        }

        [Test]
        public void CanDelete_CanNotDeleteWithIncorrectPassword()
        {
            var profile = ObjectFactory.CreateProfileView();
            profile.CurrentPassword += "1";

            Assert.IsFalse(service.CanDelete(profile));
            Assert.AreEqual(service.ModelState["CurrentPassword"].Errors[0].ErrorMessage, Validations.IncorrectPassword);
        }

        [Test]
        public void CanDelete_CanDeleteValidProfileView()
        {
            Assert.IsTrue(service.CanDelete(ObjectFactory.CreateProfileView()));
        }

        #endregion

        #region Method: Edit(ProfileView profile)

        [Test]
        public void Edit_EditsAccount()
        {
            var profileView = ObjectFactory.CreateProfileView();
            var expected = context.Set<Account>().Find(profileView.Id);
            profileView.Username += "1";
            service.Edit(profileView);

            context = new TestingContext();
            var actual = context.Set<Account>().Find(profileView.Id);

            Assert.AreEqual(expected.UserId, actual.UserId);
            Assert.AreEqual(profileView.Username, actual.Username);
            Assert.IsTrue(BCrypter.Verify(profileView.NewPassword, actual.Passhash));
        }

        [Test]
        public void Edit_EditsUser()
        {
            var profileView = ObjectFactory.CreateProfileView();
            var expected = context.Set<User>().Find(profileView.Id);
            profileView.UserDateOfBirth = null;
            profileView.UserFirstName += "1";
            profileView.UserLastName += "1";
            service.Edit(profileView);

            context = new TestingContext();
            var actual = context.Set<User>().Find(profileView.Id);

            Assert.AreEqual(profileView.UserDateOfBirth, actual.DateOfBirth);
            Assert.AreEqual(profileView.UserFirstName, actual.FirstName);
            Assert.AreEqual(profileView.UserLastName, actual.LastName);
        }

        #endregion

        #region Method: Delete(String id)
        
        [Test]
        public void Delete_DeletesAccount()
        {
            if (context.Set<Account>().Find(account.Id) == null)
                Assert.Inconclusive();

            service.Delete(account.Id);
            context = new TestingContext();

            Assert.IsNull(context.Set<Account>().Find(account.Id));
        }

        [Test]
        public void Delete_DeletesUser()
        {
            if (context.Set<User>().Find(account.UserId) == null)
                Assert.Inconclusive();

            service.Delete(account.UserId);
            context = new TestingContext();

            Assert.IsNull(context.Set<User>().Find(account.UserId));
        }

        #endregion

        #region Method: AddDeleteDisclaimerMessage()

        [Test]
        public void AddDeleteDisclaimerMessage_AddsDisclaimer()
        {
            service.AddDeleteDisclaimerMessage();
            var disclaimer = service.AlertMessages.First();

            Assert.AreEqual(disclaimer.Message, Messages.ProfileDeleteDisclaimer);
            Assert.AreEqual(disclaimer.Type, AlertMessageType.Danger);
            Assert.AreEqual(disclaimer.Key, String.Empty);
            Assert.AreEqual(disclaimer.FadeOutAfter, 0);
        }

        #endregion

        #region Test helpers

        private void SetUpData()
        {
            account = ObjectFactory.CreateAccount();
            account.User = ObjectFactory.CreateUser();
            account.UserId = account.User.Id;

            context.Set<Account>().Add(account);
            context.SaveChanges();
        }
        private void TearDownData()
        {
            var testId = TestContext.CurrentContext.Test.Name;
            foreach (var user in context.Set<User>().Where(user => user.Id.StartsWith(testId)))
                context.Set<User>().Remove(user);

            context.SaveChanges();
        }

        #endregion
    }
}
