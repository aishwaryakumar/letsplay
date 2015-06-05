using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackday
{
    public enum CommandList
    {
        ADD,
        REMOVE,
        NEXT,
        PREVIOUS,
        TOGGLEPLAYSTATE,
        NONE
    }

    public class SenderData
    {
        public event ActionRequestedHandler ActionRequested;
        public delegate void ActionRequestedHandler(Command cmd);

        public class Command
        {
            public CommandList command;
            public int songIndex;
            public byte[] data;
        }

        public void AddSongToList()
        {
            Command cmd = new Command();
            cmd.command = CommandList.ADD;
            string addRequest = JsonConvert.SerializeObject(cmd);
        }

        int charLen = 10;

        public void RemoveSong(int index)
        {
            Command cmd = new Command();
            cmd.command = CommandList.PREVIOUS;
            cmd.songIndex = index;
            string addRequest = JsonConvert.SerializeObject(cmd);
        }

        private string AppendRequestLength(string req)
        {
            int length = req.Length;
            string str = length.ToString() + req;
            int numdigits = 0;
            while(length > 0)
            {
                numdigits++;
                length = length / 10;
            }
            for(int i = 0; i < charLen-numdigits; i++)
            {
                str = "0" + str;
            }
            Debug.WriteLine(str);
            return str;
        }

        public void SendActionToServer(CommandList ActionRequested, int index = -1, byte[] data = null)
        {
            Command cmd = new Command();
            switch (ActionRequested)
            {
                case CommandList.ADD:
                    cmd.command = CommandList.ADD;
                    cmd.songIndex = index;
                    cmd.data = data;
                    break;
                case CommandList.REMOVE:
                    cmd.command = CommandList.REMOVE;
                    cmd.songIndex = index;
                    break;
                case CommandList.NEXT:
                    cmd.command = CommandList.NEXT;
                    break;
                case CommandList.PREVIOUS:
                    cmd.command = CommandList.PREVIOUS;
                    break;
                case CommandList.TOGGLEPLAYSTATE:
                    cmd.command = CommandList.TOGGLEPLAYSTATE;
                    break;
                default:
                    cmd.command = CommandList.NONE;
                    break;
            }
            string addRequest = JsonConvert.SerializeObject(cmd);
            string request = AppendRequestLength(addRequest);
        }

        public void TakeAction(string str)
        {
            Command cmd =  JsonConvert.DeserializeObject<Command>(str);
            if(cmd != null)
            {
                if (ActionRequested != null)
                    ActionRequested(cmd);
            }
        }

    }
}
