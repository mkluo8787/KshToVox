# KshToVox
  An K-Shoot-to-KFC chart converter. It is in very early alpha test (March 2018), which might be prone to error and exception.
## Usage
  1. Install .NET framework 4.7.1.
     ( Microsoft official link: https://www.microsoft.com/en-us/download/details.aspx?id=56116 )
  2. Put K-Shoot song folders into *KshSongs* subfolder in your KFC directory.
  3. Launch AutoLoad.exe with proper arguments: 
  		- *--p|path={KFC root path}*
  		- *--t|texture* (Load textures) 
  		- *--f|force-reload* (Force reload all songs)
  		- *--fm|force-meta* (Force rebuild meta db and reload songs)
  
  For Example (Force reload with texture on KFC path *D:\KFC*):  
  
```
AutoLoad.exe --path="D:\KFC" --t --f
```

  4. If no exception is raised, the K-Shoot songs should be loaded into KFC, replacing the original ones. 
  5. **Launch your game and enjoy custom charts!**
## Disclamer
  This projecct is for educational purposes only. We do not make any warranties with respect to the accuracy, applicability, fitness, or completeness of the content. **Use this at your own risk.**
