using System;
using System.Threading.Tasks;

using App1.Client.Helpers;

#if __WASM__
using WebAssembly;
using WebAssembly.Core;
#endif

namespace App1.Client.Services
{
    public class IdentityService
    {
        public event EventHandler LoggedIn;
        public event EventHandler LoggedOut;

        public bool IsAuthorized()
        {
            // TODO WTS: You can also add extra authorization checks here.
            // i.e.: Checks permisions of _authenticationResult.Account.Username in a database.
            return true;
        }

        public Task<string> GetAccessTokenForWebApiAsync() => Task.FromResult(string.Empty);

#if __WASM__
        JSObject _userManager;
        JSObject _user;

        public void Initialize()
        {
            // TODO: Fetch JSON from /_configuration/App1.Client
            var origin = Runtime.InvokeJS("window.location.origin");
            var settings = new JSObject();
            settings.SetObjectProperty("authority", origin);
            settings.SetObjectProperty("client_id", "App1.Client");
            settings.SetObjectProperty("redirect_uri", origin + "/?authentication-callback=login");
            settings.SetObjectProperty("post_logout_redirect_uri", origin + "/?authentication-callback=logout");
            settings.SetObjectProperty("response_type", "code");
            settings.SetObjectProperty("scope", "App1.ServerAPI openid profile");

            settings.SetObjectProperty("automaticSilentRenew", true);
            settings.SetObjectProperty("includeIdTokenInSilentRenew", true);

            var oidcModule = (JSObject)Runtime.GetGlobalObject("Oidc");
            var webStorageStateStoreType = (Function)oidcModule.GetObjectProperty("WebStorageStateStore");
            var userStoreArguments = new JSObject();
            userStoreArguments.SetObjectProperty("prefix", "App1.Client");
            var userStore = Runtime.NewJSObject(webStorageStateStoreType, userStoreArguments);
            settings.SetObjectProperty("userStore", userStore);

            var userManagerType = (Function)oidcModule.GetObjectProperty("UserManager");
            var userManager = Runtime.NewJSObject(userManagerType, settings);
            ((JSObject)userManager
                .GetObjectProperty("events"))
                .Invoke(
                    "addUserSignedOut",
                    (Func<Task>)(async () =>
                    {
                        await (Task)_userManager.Invoke("removeUser");
                        _user = null;
                    }));

            _userManager = userManager;
        }

        public bool IsLoggedIn()
            => _user != null;

        public async Task<LoginResultType> LoginAsync()
        {
            try
            {
                await (Task)_userManager.Invoke("signinRedirect", CreateArguments());

                return LoginResultType.Unauthorized; // Redirect? Doesn't matter.
            }
            catch (JSException ex)
            {
                Console.WriteLine("Redirect authentication error: " + ex.Message);

                return LoginResultType.UnknownError;
            }
        }

        public async Task<bool> CompleteLoginAsync(string url)
        {
            try
            {
                _user = (JSObject)await (Task<object>)_userManager.Invoke("signinCallback", url);
            }
            catch (JSException ex)
            {
                Console.WriteLine("There was an error signing in: " + ex.Message);
            }

            return _user != null;
        }

        public string GetAccountUserName()
            => (string)((JSObject)_user.GetObjectProperty("profile")).GetObjectProperty("name");

        public Task LogoutAsync()
        {
            // TODO: Implement this
            throw new NotImplementedException();
        }

        public async Task<bool> AcquireTokenSilentAsync()
        {
            try
            {
                _user = (JSObject)await (Task<object>)_userManager.Invoke("signinSilent", CreateArguments());
            }
            catch (JSException ex)
            {
                Console.WriteLine("Silent authentication error: " + ex.Message);
            }

            return _user != null;
        }

        private JSObject CreateArguments()
        {
            var arguments = new JSObject();
            arguments.SetObjectProperty("useReplaceToNavigate", true);

            return arguments;
        }
#else
        bool _isLoggedin;

        public void Initialize() { }
        public bool IsLoggedIn() => _isLoggedin;

        public Task<LoginResultType> LoginAsync()
        {
            _isLoggedin = true;

            LoggedIn?.Invoke(this, EventArgs.Empty);

            return Task.FromResult(LoginResultType.Success);
        }

        public string GetAccountUserName() => "User1";

        public Task LogoutAsync()
        {
            _isLoggedin = false;

            LoggedOut?.Invoke(this, EventArgs.Empty);

            return Task.CompletedTask;
        }

        public Task<bool> AcquireTokenSilentAsync() => Task.FromResult(false);
#endif
    }
}
