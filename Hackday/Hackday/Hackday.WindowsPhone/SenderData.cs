using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    public class SenderData
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

        public void RemoveSong(int index)
        {
            Command cmd = new Command();
            cmd.command = CommandList.PREVIOUS;
            cmd.songIndex = index;
            string addRequest = JsonConvert.SerializeObject(cmd);
        }

        public void SendActionToServer(CommandList ActionRequested, int index = -1, byte[] data = null)
        {
            Command cmd = new Command();
            switch (ActionRequested)
            {
                case CommandList.ADD:
                    cmd.command = CommandList.ADD;
                    cmd.songIndex = 0;
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
        }

        public void TakeAction(string str)
        {
            Command cmd =  JsonConvert.DeserializeObject(str) as Command;
            if(cmd != null)
            {
                switch (cmd.command)
                {
                    case CommandList.ADD:
                        break;
                    case CommandList.REMOVE:

                        break;
                    case CommandList.NEXT:
                        break;
                    case CommandList.PREVIOUS:
                        break;
                    case CommandList.TOGGLEPLAYSTATE:
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
