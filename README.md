Aimmy is a universal AI-Based Aim Alignment Mechanism developed by BabyHamsta, MarsQQ, and Taylor to make gaming more accessible for users who have difficulty aiming.
Aimmy also provides an easy to use user-interface, a wide set of features and customizability options which makes Aimmy a great option for anyone who wants to use and tailor an Aim Alignment Mechanism for a specific game without having to code.

Aimmy is 100% free to use. This means no ads, no key system, and no paywalled features. Aimmy is not, and will never be for sale for the end user, and is considered a source-available product, **not open source** as we actively discourage other developers from making commercial forks of Aimmy.

Please do not confuse Aimmy as an open-source project, we are not, and we have never been one.

Want to connect with us? Join our [Discord Server](https://discord.gg/aimmy)

If you want to share Aimmy with your friends use our [website!](https://aimmy.dev/)

# Disclaimer
This is a fork of [Aimmy](https://github.com/Babyhamsta/Aimmy/), if any problems ask us on [discord](discord.gg/aimmy).
My exe will not be as secure as Seconb's as I don't have access to Themida which is the packer he is using, if you want you can download the demo and protect it yourself.

## Features and Advantages of Arduino
- **HID Communication:** Utilizes HID instead of COM port communication, reducing detection risks in most games.
- **Easy Setup:** Straightforward script upload process to your Arduino. Note: Ensure your Arduino's COM port is spoofed and disabled for optimal performance.
- **Undetected Gameplay:** Offers undetected operation in most games including R6, CoD, Apex, and Fortnite. Detected in Valorant and CS2 FaceIt. (You will get banned on Fortnite if you don't spoof correctly. Keep that in mind.)

## Limitations of Arduino

- **USB Host Shields:** Does not support USB Host Shields. If you know what you're doing then try this: https://www.unknowncheats.me/forum/valorant/642071-arduino-hid-mouse-free-libraries.html
- **Chip Compatibility:** Specifically designed for Arduinos with an ATmega32U4 chip, such as the Leonardo R3. Other Arduinos might work by installing HoodLoader2 but the autospoofer won't work with those.

## Setup Tutorial (ARDUINO)
- [Video Tutorial](https://streamable.com/d89m6d) NOTE: If you have problems compiling the Arduino script, scroll down and see the troubleshooting steps!
- Download and install [Arduino IDE 1.8.19](https://downloads.arduino.cc/arduino-1.8.19-windows.exe)
- Download [Aimmy Arduino Edition Download](https://github.com/Seconb/Aimmy-Arduino-Edition/releases/tag/v3) and extract it
- Run arduinospoofer.exe **AS ADMIN**
- Once that finishes, open MouseInstructArduino.ino
- Next, click the upload button (the arrow pointing to the right in the top left of the Arduino IDE), wait one second, then press the red RESET button on your Arduino in real life
- If it says "Done Uploading.", you're set to continue, otherwise try again or fix whatever error it gave you.
- Run Discord_protected.exe as admin (You can also run Discord.exe, but it's less safe against anticheats because it isn't packed with Themida)
**- ALSO I HIGHLY RECOMMEND YOU MANUALLY VMPROTECT/THEMIDA Discord.exe!!! Your file will have a different md5 signature and will therefore be harder to detect. I only have 1 detection report from this on Fortnite so far but better to be safe than sorry!
- ALSO SOLID BAN CHANCE ON BE/EAC IF YOU DON'T DISABLE COM PORT**
- ## Troubleshooting (ARDUINO)
- If the Arduino script doesn't compile I would double check that you're NOT using the Windows Store version of Arduino IDE and you are using the one from the link in the setup tutorial.
- If you get the issue where everything says "Unspecified" then spoof your Arduino again but do not choose to disable COM port. Then, upload MouseInstructArduino again. It should work now but it's more likely to get detected in some games in the future. This is unfortunately the only known fix.
- If you get No Such Directory "HID-Settings.h" (or any No Such Directory Error) then press CTRL+Shift+I in the Arduino IDE, search for HID-Project, and install it. Then, try again. If that doesn't work, try replacing the MouseInstructArduino folder with the one from here (replace nothing else, only MouseInstructArduino): https://github.com/Seconb/Aimmy-Arduino-Edition/releases/download/v1/Aimmy.Arduino.v2.1.rar
- If your script doesn't compile, spoof your Arduino again using the Arduino spoofer and then go to %programfiles(x86)%\Arduino\hardware\arduino\avr\ by copy and pasting that into the Windows Search bar. Then, copy the boards.txt from there to %localappdata%\Arduino15\packages\arduino\hardware\avr\1.8.6 by copy and pasting it into the Windows Search bar. Next, right click the boards.txt files, go to properties, and check "Read-only". Save that. If you need to spoof your Arduino again, uncheck "Read-only" on both boards.txt files before doing so. It's really important that both boards.txt files are the same so confirm that they are and both are spoofed. You really have to make sure you picked the right mouse in the spoofer.
- If you have any other issue consider watching the video extra carefully and redoing it. If all else fails idk what to tell you because I don't want to help you on Discord. This isn't for some inexperienced users this is for people who desperately need Arduino for Aimmy before it ever becomes an official update.
- If your Arduino has issues working in general, try using a different USB port and a better cable. I recommend the one that comes with the Arduino. A lot of times, using some random cable you have somewhere is bad because they aren't made for something as powerful as an Arduino
- If COM ports aren't disabled after spoofing, make sure you have Arduino IDE installed correctly. If it is installed correctly, download this: https://github.com/Seconb/Aimmy-Arduino-Edition/releases/download/v1/USBCore.cpp  then go to C:\Program Files (x86)\Arduino\hardware\arduino\avr\cores\arduino and replace USBCore.cpp with the one you downloaded. Do the same for C:\Users\karab\AppData\Local\Arduino15\packages\arduino\hardware\avr\1.8.6\cores\arduino if you have that folder (not everyone has it).

## What is CUDA
> **What's CUDA?**

```Cuda is pretty much just the better version of "DirectML" and uses Nividia's GPU power to make it more smoother and faster```

> **What's DirectML?**

```Think of it as a mid lvl AI that relies on your GPU to work good```

> **How does the AI work?**

```Using the imported models (pictures), it will then scan the game as you play and look for players that match the models (pictures)```

## Setup (CUDA automatically)
- Download and run LazyAimmySetupCuda ... thats it.

### Install LazyAimmySetup

[Dropbox](https://www.dropbox.com/scl/fi/impeduswsqr04uq59g4kj/LazyCudaAimmySetup.exe?rlkey=ka4oh9r12a7jqqbesild1ehu0&st=cxp33hin&dl=0)

[4Shared](https://www.4shared.com/file/79lMDawZku/LazyCudaAimmySetup__2_.html)

[PixelDrain](https://pixeldrain.com/u/NAhaSbKL)


https://github.com/user-attachments/assets/0df379d1-958e-4e69-b6e2-24c5d8a7b7fd




## Setup (CUDA manually)
- Download and Install the x64 version of [.NET Runtime 8.0.X.X](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.2-windows-x64-installer)
- Download and Install the x64 version of [.NET Runtime 7.0.X.X](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.20-windows-x64-installer)
- Download and Install the x64 version of [Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe)
- Download Aimmy from [Releases](https://github.com/TaylorIsBlue/Aimmy-CUDA/releases) (Make sure it's the Aimmy zip and not Source zip)
- **Get [cuDNN 8.9.x](https://developer.nvidia.com/rdp/cudnn-archive) and [CUDA 11.8](https://developer.nvidia.com/cuda-11-8-0-download-archive)**
- Extract the Aimmy.zip file
- Run Aimmy.exe
- Choose your Model and Enjoy :)
### Troubleshooting CUDA
Sometimes, when you load a model the application closes in an exception, this could mean:
1. Your cuda installation is wrong. Check your PATH (env variables) for your Cuda installation and your cuDNN.
2. Download and Install CUDA and cuDNN of [CUDA 12.x](https://developer.nvidia.com/cuda-downloads) and [cuDNN 9.x](https://developer.nvidia.com/cudnn-downloads), maybe you need to switch versions im not sure its kind of weird.

## Credits
- [MouseInstruct Repository](https://github.com/khanxbahria/MouseInstruct) for their amazing HID library. Made mouse movement via Arduino easy.
- Seconb (Arduino)
- Befia/Taylor (CUDA Aimmy)
- The Aimmy Developers, because this is just Aimmy with some mods.
