SendKeys sends keystrokes entered into the console window to the target computer.

Switches are as follows:

SendKeys ip-address -[Sender:Receiver]

-Sender: Takes keystrokes entered into the app and sends them to ip-address

-Receiver: Receives from any ipaddress on 8123 and sends key to the screen



Limitations:

While it will send all keystrokes (backspace, tab, etc) it will not send certain commands due to limitations of console. 
One example is CTRL+C. If you run into another limitation let me know and ill add it to the list.

In order to not make a keylogger, the app needs to be the active app on the screen to capture keystrokes.
