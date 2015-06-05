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

    public class SongData
    {
        public byte[] SongByteArray;
        public string Name;
    }

    public class Command
    {
        public CommandList command;
        public int songIndex;
        public SongData SongData;
    }

    public class SenderData
    {
        public event ActionRequestedHandler ActionRequested;
        public delegate void ActionRequestedHandler(Command cmd);       
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
            return str;
        }

        public void SendActionToServer(CommandList ActionRequested, string name = "", int index = -1, byte[] data = null)
        {
            Command cmd = new Command();
            switch (ActionRequested)
            {
                case CommandList.ADD:
                    cmd.command = CommandList.ADD;
                    cmd.songIndex = index;
                    SongData songData = new SongData() { SongByteArray = data, Name = name };
                    cmd.SongData = songData;
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
            ConnectionManager.Instance.SendData(request);
        }
    }
}
