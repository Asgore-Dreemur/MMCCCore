# MMCCCore
## 简介
MMCCCore是一个为Modern Minecraft Client所开发的启动器核心  
但本身并不局限与Modern Minecraft Client,任何启动器均可调用该项目  
主开发者为Bit  
虽然这个项目几乎只有我一个人开发,但它是一个团队项目  
该项目属于InnoSofts,缩写ist,室长LiveTiles  
## 声明 & 引用
>BMCLAPI是@bangbang93开发的BMCL的一部分，用于解决国内线路对Forge和Minecraft官方使用的Amazon S3 速度缓慢的问题。BMCLAPI是对外开放的，所有需要Minecraft资源的启动器均可调用。

>本项目部分参考自FluentCore,这是一个不错的启动器核心,也可以去看看他的代码:https://github.com/Xcube-Studio/Natsurainko.FluentCore/

>感谢 MC启动器交流（MCLauncher(China) Foundation ） 和 InnoSofts工作室 的所有成员,为本项目解答了很多的疑问,提出了宝贵的建议

>本项目使用了Newtonsoft.Json,System.Security.Permissions和System.IO.Compression.ZipFile
## 架构与环境
+ 该项目现在还不是一个跨平台项目,仅限于Windows,但以后可能会改变成为一个跨平台项目(改是很容易的)  
+ 运行于.Net Core 3.1/.Net Framework 4.7
## 功能
+ 支持Microsoft,离线,外置登录(不支持Mojang)  
+ 下载Minecraft本体与安装各种ModAPI(不支持Quilt)
+ 最基本的启动游戏
+ 支持Curseforge与Modrinth的资源查询&下载
+ ...
## 安装
+ 直接下载项目并引用
+ 下载发行版里的zip并解压
+ 将项目编译为dll后引用
