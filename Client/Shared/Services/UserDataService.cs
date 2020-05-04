using System;
using System.Threading.Tasks;

using App1.Client.Helpers;
using App1.Client.Models;

namespace App1.Client.Services
{
    public class UserDataService
    {
        private UserData _user;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        public event EventHandler<UserData> UserDataUpdated;

        public UserDataService()
        {
        }

        public void Initialize()
        {
            IdentityService.LoggedIn += OnLoggedIn;
            IdentityService.LoggedOut += OnLoggedOut;
        }

        public Task<UserData> GetUserAsync()
        {
            if (_user == null)
            {
                _user = GetDefaultUserData();
            }

            return Task.FromResult(_user);
        }

        private void OnLoggedIn(object sender, EventArgs e)
        {
            UserDataUpdated?.Invoke(this, _user);
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            _user = null;
        }

        private UserData GetDefaultUserData()
        {
            return new UserData()
            {
                Name = IdentityService.GetAccountUserName(),
                Photo = ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
            };
        }
    }
}
