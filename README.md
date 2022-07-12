# What
- Facebook SDK 14.0.0

# Environment
- Project open with unity 2021.3 LTS
- Support unity 2019+

# How To Install

Add the lines below to `Packages/manifest.json`

for version 1.0.0
```csharp
"com.pancake.facebook": "https://github.com/pancake-llc/facebook.git?path=Assets/_Root#1.0.0",
"com.google.external-dependency-manager": "https://github.com/pancake-llc/external-dependency-manager.git?path=Assets/_Root#1.2.169",
"com.pancake.common": "https://github.com/pancake-llc/common.git?path=Assets/_Root#1.2.3",
```


# Notes
- Facebook application need create with type is `gaming`
- If permission `gaming_user_picture` not include will return avartar, if include it will return profile picture

```cs
    public Image prefab;
    public Transform root;
    private async void Start()
    {
        if (FacebookManager.Instance.IsLoggedIn)
        {
            FacebookManager.Instance.GetMeProfile(FacebookManager.Instance.OnGetProfilePhotoCompleted);

            await UniTask.WaitUntil(() => !FacebookManager.Instance.IsRequestingProfile);
            var o = Instantiate(prefab, root);
            o.sprite = FacebookManager.CreateSprite(FacebookManager.Instance.ProfilePicture, Vector2.one * 0.5f);
        }
    }
    

    public void Login() { FacebookManager.Instance.Login(OnLoginCompleted, OnLoginFaild, OnLoginError); }

    private void OnLoginError() { }

    private void OnLoginFaild() { }

    private async void OnLoginCompleted()
    {
        await UniTask.WaitUntil(() => !FacebookManager.Instance.IsRequestingProfile);
        var o = Instantiate(prefab, root);
        o.sprite = FacebookManager.CreateSprite(FacebookManager.Instance.ProfilePicture, Vector2.one * 0.5f);
        
        FacebookManager.Instance.GetMeFriend();

        await UniTask.WaitUntil(() => !FacebookManager.Instance.IsRequestingFriend);
        var p = FacebookManager.Instance.LoadProfileAllFriend();
        await p;
        for (int i = 0; i < FacebookManager.Instance.FriendDatas.Count; i++)
        {
            var result = Instantiate(prefab, root);
            Debug.Log("friend : "  + FacebookManager.Instance.FriendDatas[i].name);
            result.sprite = FacebookManager.CreateSprite(FacebookManager.Instance.FriendDatas[i].avatar, Vector2.one * 0.5f);
        }
    }
```