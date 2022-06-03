# What
- Facebook SDK 11.0.0

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