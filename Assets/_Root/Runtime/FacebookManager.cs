using Cysharp.Threading.Tasks;
using Facebook.Unity;
using MEC;
using SimpleJSON;
using UnityEngine.Networking;

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

        public bool publicProfile = true;
        public bool email = true;
        public bool gamingProfile;
        public bool gamingUserPicture;
        public bool userAgeRange;
        public bool userBirthday;
        public bool userFriends;
        public bool userGender;
        public bool userHometown;
        public bool userLink;
        public bool userLocation;
        public bool userMessengerContact;

        public LoginTracking typeLogin = LoginTracking.ENABLED;

        private Texture2D _profilePicture;
        private bool _isRequestingProfile;
        private bool _isRequestingFriend;
        public bool IsInitialized => FB.IsInitialized;
        public bool IsLoggedIn => FB.IsLoggedIn;
        private CoroutineHandle _coroutineHandle;
        private List<FriendData> _friendDatas;

        public string UserId { get; private set; }
        public string Token { get; private set; }

        public Texture2D ProfilePicture => _profilePicture;

        public bool IsRequestingProfile => _isRequestingProfile;

        public bool IsRequestingFriend => _isRequestingFriend;

#if UNITY_IOS
        public string UserName { get; private set; }
        public string UserEmail { get; private set; }
        public string ProfileImageUrl { get; private set; }
        public DateTime? UserBirthday { get; private set; }
        public UserAgeRange UserAgeRange { get; private set; }
        public string[] UserFriendIds { get; private set; }
#endif

        #region Init

        private void Awake()
        {
            UserId = "";
            Token = "";

            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(OnInitCompleted, OnHideUnity);
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }
        }

        private void OnInitCompleted()
        {
            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();

                if (IsLoggedIn)
                {
#if UNITY_IOS
                    GetProfileInfo();
#endif
                    var token = AccessToken.CurrentAccessToken;
                    UserId = token.UserId;
                    Token = token.TokenString;
                }
            }
            else
            {
                //todo
                Debug.Log("Failed to Initialize the Facebook SDK");
            }
        }

        private void OnHideUnity(bool isGameShown) { Time.timeScale = !isGameShown ? 0 : 1; }

        #endregion

#if UNITY_IOS
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
#endif

        private void GetMeProfile(Action<IGraphResult> successCallback = null, Action<IGraphResult> errorCallback = null, int width = 128, int height = 128)
        {
            GetFacebookUserPicture("me",
                width,
                height,
                successCallback,
                errorCallback);
        }

        public void GetFacebookUserPicture(string id, int width, int height, Action<IGraphResult> successCallback = null, Action<IGraphResult> errorCallback = null)
        {
            if (!IsLoggedIn) return;
            _isRequestingProfile = true;
            string query = string.Format("/{0}/picture?type=square&height={1}&width={2}", id, height, width);
            FB.API(query,
                HttpMethod.GET,
                result =>
                {
                    _isRequestingProfile = false;
                    if (result == null || result.Error != null)
                    {
                        errorCallback?.Invoke(result);
                        return;
                    }

                    successCallback?.Invoke(result);
                });
        }

        private void OnGetProfilePhotoCompleted(IGraphResult result)
        {
            if (result.Texture != null) _profilePicture = result.Texture;
        }

        public static Sprite CreateSprite(Texture2D texture2D, Vector2 pivot)
        {
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), pivot);
        }

        public void GetMeFriend()
        {
            if (!IsLoggedIn) return;
            _isRequestingFriend = true;
            FB.API("/me/friends", HttpMethod.GET, OnGetFriendCompleted);
        }

        private void OnGetFriendCompleted(IGraphResult result)
        {
            _isRequestingFriend = false;
            if (result == null || result.Error != null) return;

            var jsonNode = JSON.Parse(result.RawResult);
            var data = jsonNode["data"];

            _friendDatas = new List<FriendData>();
            for (int i = 0; i < data.Count; i++)
            {
                _friendDatas.Add(new FriendData {id = data[i]["id"].ToString(), name = data[i]["name"].ToString(), pictureUrl = data[i]["picture"]["data"]["url"]});
            }
        }

        private async UniTask<Texture2D> LoadTextureInternal(string url)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);
            request.timeout = 120;

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[TextureLoad]Download Texture Failed : {request.error}", this);

                return default;
            }

            var handler = request.downloadHandler as DownloadHandlerTexture;

            return handler?.texture;
        }

        #region login

        private async void LoadProfileAllFriend()
        {
            await UniTask.WaitUntil(() => !_isRequestingFriend && _friendDatas != null);

            var avatars = new List<Texture2D>();
            foreach (var friend in _friendDatas)
            {
                var result = await LoadTextureInternal(friend.pictureUrl);
                avatars.Add(result);
            }
        }

        public void Login(Action onComplete = null, Action onFaild = null, Action onError = null)
        {
            onLoginComplete = onComplete;
            onLoginFaild = onFaild;
            onLoginError = onError;
            var scopes = new List<string>();
            if (publicProfile) scopes.Add("public_profile");
            if (email) scopes.Add("email");
            if (gamingProfile) scopes.Add("gaming_profile");
            if (gamingUserPicture) scopes.Add("gaming_user_picture");
            if (userFriends) scopes.Add("user_friends");
            if (userBirthday) scopes.Add("user_birthday");
            if (userAgeRange) scopes.Add("user_age_range");
            if (userLocation) scopes.Add("user_location");
            if (userHometown) scopes.Add("user_hometown");
            if (userGender) scopes.Add("user_gender");
            if (userLink) scopes.Add("user_link");
            if (userMessengerContact) scopes.Add("user_messenger_contact");

            FB.Mobile.LoginWithTrackingPreference(typeLogin, scopes, "pancake_nonce25", HandleResult);
        }

        private void HandleResult(ILoginResult result)
        {
            if (result == null) return;

            if (result.Error != null)
            {
                onLoginError?.Invoke();
                return;
            }

            if (IsLoggedIn)
            {
#if UNITY_IOS
                GetProfileInfo();
#endif
                // AccessToken class will have session details
                var token = AccessToken.CurrentAccessToken;
                UserId = token.UserId;
                Token = token.TokenString;
                GetMeProfile(OnGetProfilePhotoCompleted);
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

        public struct FriendData
        {
            public string id;
            public string name;
            public string pictureUrl;
        }
    }
}