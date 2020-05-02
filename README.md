# AutumnShell
Custom shell for Windows 7 made in C# using WPF and .NET 4.0  
Tested on Windows 7 SP1 x86 and x64.

Thanks to ChrisP1118 for his work on [making global system hooks possible on C#](https://www.codeproject.com/Articles/18638/Using-Window-Messages-to-Implement-Global-System-H) and Alexei Stryker for [making ChrisP1118's project work on WPF](https://legacyofvoid.wordpress.com/2011/11/16/global-system-hooks-in-c/).  
GlobalSystemHooks for C# (source of GlobalHooks.dll): https://github.com/MLXProjects/GlobalSystemHooks
  
![Image](https://i.ibb.co/TRw1FYx/autumn-img.png)  
  
# Description:  
It's just a custom Windows shell, which can be used as replacement for explorer.exe.  
It's made in C# using WPF, so don't expect the best performance ever nor the most feature-rich shell.  
Currently, it integrates:  
- Basic start-like apps menu  
- File and folder opening (it also integrates it's own file manager :D)  
- Taskbar and open windows list with closing/opening windows detection  
- Wallpaper changing and previewing  
  
# Things broken/to be implemented:
- [ ] Highlight the foreground window's taskbar button (currently broken, it half works) 
- [ ] Implement context menu on bottom panel, apps menu and desktop (the last one is incomplete)
- [ ] Implement the file manager in order to stop relying on explorer.exe (useful if you want to fully remove it)
- [ ] Make the explorer's close at startup and open at shell's closing optional (if you don't have explorer.exe the program simply crashes)
  
# How to compile
For working on this project, I'm using  Visual Studio 2010 (pretty old, cause of potato pc) and targetting .NET Framework 4.0.  
It's only tested on that IDE using Windows 7 32-bit, so it may (surely will) not work on anoter environment/OS without some changes.
