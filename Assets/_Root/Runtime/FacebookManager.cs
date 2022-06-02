using Facebook.Unity;
using MEC;

namespace Pancake.Facebook
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class FacebookManager : MonoBehaviour
    {
        public Action onLoginComplete;
        public Action onLoginFaild;
        public Action onLogoutComplete;
        public Action onLoginError;

        public bool IsInitialized => FB.IsInitialized;
        public bool IsLoggedIn => FB.IsLoggedIn;
        private CoroutineHandle _coroutineHandle;

        public string UserId { get; private set; }
        public string Token { get; private set; }
        public string UserName { get; private set; }
        public string UserEmail { get; private set; }
        public string ProfileImageUrl { get; private set; }
        public DateTime? UserBirthday { get; private set; }
        public UserAgeRange UserAgeRange { get; private set; }
        public string[] UserFriendIds { get; private set; }


        // Awake function from Unity's MonoBehavior
        private void Awake()
        {
            Debug.Log("");
            UserId = "";
            Token = "";

            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }
        }

        public void GetProfileInfo()
        {
            if (IsLoggedIn)
            {
                var profile = FB.Mobile.CurrentProfile();
                if (profile != null)
                {
                    UserName = profile.Name;
                    UserId = profile.UserID;
                    UserEmail = profile.Email;
                    ProfileImageUrl = profile.ImageURL;
                    UserBirthday = profile.Birthday;
                    UserAgeRange = profile.AgeRange;
                    UserFriendIds = profile.FriendIDs;
                }
            }
        }

        #region login

        public void Login(Action onComplete = null, Action onFaild = null, Action onError = null)
        {
            onLoginComplete = onComplete;
            onLoginFaild = onFaild;
            onLoginError = onError;
            var permissions = new List<string> { "gaming_profile", "email", "user_friends", "gaming_user_picture", "gaming_user_locale" };
            FB.Mobile.LoginWithTrackingPreference(LoginTracking.LIMITED, permissions, callback: AuthCallback);
        }

        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
                // ...

                if (IsLoggedIn)
                {
                    GetProfileInfo();

                    // todo load.
                    var token = AccessToken.CurrentAccessToken;
                    UserId = token.UserId;
                    Token = token.TokenString;
                }
            }
            else
            {
                //todo Debug.Log("Failed to Initialize the Facebook SDK");
            }
        }

        private void OnHideUnity(bool isGameShown) { Time.timeScale = !isGameShown ? 0 : 1; }

        private void AuthCallback(ILoginResult result)
        {
            if (result.Error != null)
            {
                onLoginError?.Invoke();
                // todo error login
                return;
            }

            if (IsLoggedIn)
            {
                GetProfileInfo();
                Debug.Log("userId1 :" + UserId);
                // AccessToken class will have session details
                var token = AccessToken.CurrentAccessToken;
                UserId = token.UserId;
                Debug.Log("userId2 :" + UserId);
                Token = token.TokenString;
                // todo aToken.TokenString;
                onLoginComplete?.Invoke();
                onLoginComplete = null;
            }
            else
            {
                //todo User cancelled login
                onLoginFaild?.Invoke();
                onLoginFaild = null;
            }
        }

        #endregion

        #region logout

        public void Logout(Action onComplete = null)
        {
            onLogoutComplete = onComplete;
            if (FB.IsLoggedIn)
            {
                FB.LogOut();
                _coroutineHandle = Timing.RunCoroutine(IeLogoutSuccess());
            }
        }

        private IEnumerator<float> IeLogoutSuccess()
        {
            if (FB.IsLoggedIn)
            {
                yield return Timing.WaitForSeconds(0.1f);
                Timing.KillCoroutines(_coroutineHandle);
                _coroutineHandle = Timing.RunCoroutine(IeLogoutSuccess());
            }
            else
            {
                onLogoutComplete?.Invoke();
                onLogoutComplete = null;
                Token = "";
                UserId = "";
            }
        }

        #endregion
    }
}