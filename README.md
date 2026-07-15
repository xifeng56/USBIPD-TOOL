This is a WSL USB sharing program

Use config.txt add config

Its regular expression is as follows

@$"^(?!.*Attached)(\d+-\d+)\s.*(?:{newLine})"

First, you need install usbipd

Then, use the usbipd list command in the terminal to find device.

<img width="848" height="228" alt="image" src="https://github.com/user-attachments/assets/aefb316a-9913-4995-bf51-e6025af5c272" />

If you want sharing "USB Serial Converter",You need add "Conv" (you can read regular expression) in the congfig.txt, Use enter add next config.

这是一个WSL的USB共享项目

使用config.txt添加配置

它的正则表达如下

@$"^(?!.*Attached)(\d+-\d+)\s.*(?:{newLine})"

首先,你需要安装usbipd

然后在终端使用usbipd list命令查看usb设备列表

<img width="848" height="228" alt="image" src="https://github.com/user-attachments/assets/aefb316a-9913-4995-bf51-e6025af5c272" />

如果你要共享 "USB Serial Converter",你需要在config.txt中添加"Conv"(具体参考正则表达式),回车后添加下一条配置
