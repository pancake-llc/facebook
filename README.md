# What
- Facebook SDK 14.1.0

# Environment
- Project open with unity 2021.3 LTS
- Support unity 2021+

# How To Install

Add the lines below to `Packages/manifest.json`

for version 1.0.7
```csharp
"com.pancake.facebook": "https://github.com/pancake-llc/facebook.git?path=Assets/_Root#1.0.7",
"com.google.external-dependency-manager": "https://github.com/pancake-llc/external-dependency-manager.git?path=Assets/_Root#1.2.169",
```

Dependency : [heart](!https://github.com/pancake-llc/heart)
```csharp
"com.pancake.heart": "https://github.com/pancake-llc/heart.git?path=Assets/_Root",
"com.system-community.ben-demystifier": "https://github.com/system-community/BenDemystifier.git?path=Assets/_Root#0.4.1",
"com.system-community.harmony": "https://github.com/system-community/harmony.git?path=Assets/_Root#2.2.2",
"com.system-community.stringtools": "https://github.com/system-community/StringTools.git?path=Assets/_Root#1.0.0",
"com.system-community.reflection-metadata": "https://github.com/system-community/SystemReflectionMetadata.git?path=Assets/_Root#5.0.0",
"com.system-community.systemcollectionsimmutable": "https://github.com/system-community/SystemCollectionsImmutable.git?path=Assets/_Root#5.0.0",
"com.system-community.systemruntimecompilerservicesunsafe": "https://github.com/system-community/SystemRuntimeCompilerServicesUnsafe.git?path=Assets/_Root#5.0.0",
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