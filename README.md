
<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
An Escape From Tarkov BepInEx module designed to be used with SPT-Aki Server with the ultimate goal of "Offline" Coop 
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/paulov-t/SIT.Core/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/latest/total)

[English](README.md) **|** [简体中文](README_CN.md) **|** [Deutsch](README_DE.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md) **|** [한국어-Korean](README_KO.md) **|** [Français](README_FR.md)
</div>

---

## State of Stay In Tarkov

* SPT-Aki 3.7.2 is available on their website
* Stay In Tarkov has entered a state of contributor development (i.e. other people are contributing more than I)
* There are some bugs I cannot resolve or require signifcant rewrites to BSG code and BSG change their code with almost every patch
* I don't play offline anymore as this project was made for my Tarkov group but then they decided to not want to play it or Live (got bored of Tarkov in general)
* I aim to keep updating this project with each BSG update and plan to possibly support Arena or attempt to create our own Arena, which would be cool
* Pull Requests and Contributions will always be accepted (if they work!)

--- 

## About

The Stay in Tarkov project was born due to Battlestate Games' (BSG) reluctance to create the pure PvE version of Escape from Tarkov. 
The project's aim is simple, create a Cooperation PvE experience that retains progression. 
If BSG decide to create the ability to do this on live OR I receive a DCMA request, this project will be shut down immediately.

## Disclaimer

* You must buy the game to use this. You can obtain it here. [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com). 
* This is by no means designed for cheats (this project was made because cheats have destroyed the Live experience)
* This is by no means designed for illegally downloading the game (and has blocks for people that do!)
* This is purely for educational purposes. I am using this to learn Unity and TCP/UDP/Web Socket Networking and I learnt a lot from BattleState Games \o/.
* I am not affiliated with BSG or others (on Reddit or Discord) claiming to be working on a project. Do NOT contact SPTarkov subreddit or Discord about this project.
* This project is not affiliated with SPTarkov but uses its excellent Server.
* I do not offer support. This project comes "as-is". It either works for you or it doesn't.

## Support

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* **I do not offer support. All tutorials written by me or any other contributors are out of good faith. If you cannot get this working, I suggest going back to Live!**
* The Ko-Fi link is buying me (or my wife) a coffee, nothing else! 
* Pull Requests are encouraged. Thanks to all contributors!
* Please do not hand over money expecting help or a solution. 
* This is a hobby, for fun, project. Please don't treat it seriously. 
* I know this is a semi-broken attempt but will try to fix as best I can. 
* An [Unofficial SIT Discord](https://discord.gg/VengzHxNmZ) is available. The community have teamed to help each other out and create community servers. **I am not part of this Discord**.

## SPT-AKI Requirement
* Stay in Tarkov works requires the [latest AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Server) to run. You can learn about SPT-Aki [here](https://www.sp-tarkov.com/).
* DO NOT INSTALL THIS ON TO SPT-Aki CLIENT! ONLY INSTALL THE SERVER!

## [Wiki](https://github.com/paulov-t/SIT.Core/wiki)
**The Wiki is has been constructed by various contributors. All instructions are also kept within the source in the wiki directory.**
  - ### [Setup Manuals](https://github.com/paulov-t/SIT.Core/wiki/Guides-English)
  - ### [FAQs](https://github.com/paulov-t/SIT.Core/wiki/FAQs-English)

## Coop

### Coop Summary
**BE AWARE**
* Coop is in early stages of development. 
* Most features work (ish) and it is "playable (ish) with likely bugs". "Playable" and perfect are two very different things. Expect lag (desync), issues and bugs.
* The Host & Server must have a good stable connection with an upload speed of at least 5-10mbps. The AI take a lot of CPU & Network bandwidth to run.
* Despite many people saying otherwise. You can play with people across the world (not just LAN). I have played with people with over 200 ping. They get lag similar to live, just shown in a different way.
* Despite claims that "VPN"s like HAMACHI/RADMIN work. I highly recommend you do not use them. They have very slow connections. Always try to find a way to host directly OR pay for a cheap server to host the Aki Server.

### PREREQUISITE
You must have the [SPT-Aki mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) installed in your Server for this module to work. If you do not wish to use the Coop module, you must disable it in the BepInEx config file.

### Can Coop use BSG's Coop code?
No. BSG server code is hidden from the client for obvious reasons. So BSG's implementation of Coop use the same online servers as PvPvE. We don't see this, so we cannot use this.

### Coding explanation
- The project uses multiple methods of BepInEx Harmony patches coupled with Unity Components to achieve its aims.
- Features/Methods that require constant polling between Client->Server->Client (Move, Rotate, Look, etc) use Components to send data (AI code runs the Update/LateUpdate command and the function every tick, therefore causing network flood).
- Features/Methods that can easily be "replicated" use ModuleReplicationPatch abstract class to easily round trip the call.
- All server communication is via JSON TCP Http and Web Socket calls to the ["Web Server" developed by SPT-Aki](https://dev.sp-tarkov.com/SPT-AKI/Server) using a [typescript mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) to handle the "backend" work.
- CoopGameComponent is attached to the GameWorld object when a Coop ready game is started (any game that isn't Hideout). CoopGameComponent polls the Server for information and passes the data to the PlayerReplicatedComponent.

## SPT-Aki

### Are Aki BepInEx (Client mods) Modules supported?
The following Aki Modules are supported.
- aki-core
- Aki.Common
- Aki.Reflection
- Do SPT-AKI Client mods work? This is dependant on how well written the patches are. If they directly target GCLASSXXX or PUBLIC/PRIVATE then they will likely fail.

### Why don't you use Aki Module DLLs?
SPT-Aki DLLs are written specifically for their own Deobfuscation technique and my own technique is not working well with Aki Modules at this moment in time.
So I ported many of SPT-Aki features into this module. My end-goal would be to rely on SPT-Aki and for this to be solely focused on SIT only features.

## How to compile? 
[Compiling Document](COMPILE.md)

## Thanks List
- SPT-Aki team
- MTGA team
- SPT-Aki Modding Community
- DrakiaXYZ ([BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain))
- Dvize ([NoBushESP](https://github.com/dvize/NoBushESP))
- SIT Contributors

## License

- DrakiaXYZ projects contain the MIT License
- 95% of the original core and single-player functionality completed by SPT-Aki teams. There may be licenses pertaining to them within this source.
- None of my own work is Licensed. This is solely a just for fun project. I don't care what you do with it.
