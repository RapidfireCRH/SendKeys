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
                        string msg = SerializeObject<key_st>(send);
                        Console.Clear();
                        net.send(ip, msg);
                        if(args[2]=="-synccp")
                        {
                            if(send.key == ConsoleKey.F10 && send.cm.ToString().Contains("Control"))
                            {
                                Thread.Sleep(10);
                                _net._rec data = net.receive();
                                
                                cp_st rec_cp = DeserializeObject<cp_st>(data.message);
                                if (rec_cp.clipboard_contents == "")
                                    continue;
                                Clipboard.SetText(rec_cp.clipboard_contents);
                            }
                        }
                    }
                    break;
                case "-receiver":
                    while (true)
                    {
                        _net._rec rec = net.receive();
                        if (rec.message == "")
                            continue;
                        key_st recieved_msg = DeserializeObject<key_st>(Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(rec.message)));
                        if (args[2] == "-synccp")
                        {
                            if (recieved_msg.key == ConsoleKey.F10 && recieved_msg.cm.ToString().Contains("Control"))
                            {
                                key_st temp = new key_st();
                                temp.key = ConsoleKey.C;
                                temp.cm = ConsoleModifiers.Control;
                                Sender(temp);
                                cp_st send_cp = new cp_st();
                                send_cp.clipboard_contents = Clipboard.GetText();
                                net.send(rec.ip, SerializeObject<cp_st>(send_cp));
                            }
                        }

                        if (Sender(recieved_msg) && !(recieved_msg.key == ConsoleKey.F10 && recieved_msg.cm.ToString().Contains("Control")))
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
                if (send.cm.ToString().Contains("Control"))
                    modkey.Add(VirtualKeyCode.CONTROL);
                if (send.cm.ToString().Contains("Alt"))
                    modkey.Add(VirtualKeyCode.MENU);
                if (send.cm.ToString().Contains("Shift"))
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

        public static string SerializeObject<T>(T objectToSerialize)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream memStr = new MemoryStream();

            try
            {
                bf.Serialize(memStr, objectToSerialize);
                memStr.Position = 0;

                return Convert.ToBase64String(memStr.ToArray());
            }
            finally
            {
                memStr.Close();
            }
        }

        public static T DeserializeObject<T>(string str)
        {
            BinaryFormatter bf = new BinaryFormatter();
            byte[] b = Convert.FromBase64String(str);
            MemoryStream ms = new MemoryStream(b);

            try
            {
                return (T)bf.Deserialize(ms);
            }
            finally
            {
                ms.Close();
            }
        }
    }
}
