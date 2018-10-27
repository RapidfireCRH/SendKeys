using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace SendKey
{
    static class Program
    {
        static _net net = new _net();
        [Serializable]
        public struct key_st
        {
            public ConsoleKey key;
            public ConsoleModifiers cm;
        }
        public static key_st send = new key_st();
        [Serializable]
        public struct cp_st
        {
            public string clipboard_contents;
        }
        [STAThread]
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse(args[0]);
            switch(args[1])
            {
                case "-sender":
                    ConsoleKeyInfo press;
                    while ((press = Console.ReadKey()).Key != ConsoleKey.Escape)
                    {
                        VirtualKeyCode keyvs = (VirtualKeyCode)press.Key;
                        send.key = press.Key;
                        send.cm = press.Modifiers;
                        string msg = net.SerializeObject<key_st>(send);
                        Console.Clear();
                        net.send(ip, msg);
                    }
                    break;
                case "-receiver":
                    while (true)
                    {
                        _net._rec rec = net.receive();
                        if (rec.message == "")
                            continue;
                        key_st recieved_msg = net.DeserializeObject<key_st>(Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(rec.message)));

                        if (Sender(recieved_msg))
                            Console.WriteLine("Error sending key.");
                    }
            }
        }

        private static bool Sender(key_st received_msg)
        {
            try
            {
                InputSimulator sim = new InputSimulator();
                List<VirtualKeyCode> modkey = new List<VirtualKeyCode>();
                if (received_msg.cm.ToString().Contains("Control"))
                    modkey.Add(VirtualKeyCode.CONTROL);
                if (received_msg.cm.ToString().Contains("Alt"))
                    modkey.Add(VirtualKeyCode.MENU);
                if (received_msg.cm.ToString().Contains("Shift"))
                    modkey.Add(VirtualKeyCode.SHIFT);
                if (modkey.Count == 0)
                    sim.Keyboard.ModifiedKeyStroke(0, (VirtualKeyCode)received_msg.key);
                else
                    sim.Keyboard.ModifiedKeyStroke(modkey, (VirtualKeyCode)received_msg.key);
                return false;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return true;
            }
        }

        
    }
}
