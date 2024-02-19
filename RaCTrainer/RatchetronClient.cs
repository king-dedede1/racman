using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace racman
{
    internal class RatchetronClient : IMemoryAPI
    {
        private Ratchetron api;

        public RatchetronClient(IPAddress ip)
        {
            api = new Ratchetron(ip.ToString());
            api.Connect();
        }

        public void Disconnect()
        {
            api.Disconnect();
        }

        public int FreezeMemory(uint addr, byte[] value)
        {
            return api.FreezeMemory(api.getCurrentPID(), addr, (uint)value.Length, Ratchetron.MemoryCondition.Any, value);
        }

        public string GetGameTitle()
        {
            // Yep
            string html = new WebMAN(this.api.GetIP()).CPURSX();
            int loc = html.IndexOf("http://google.com/search?q=");
            if (loc == -1)
            {
                return "PS3 Game";
            }
            else
            {
                int i = 0;
                for (; html[i + loc + 27] != '\"'; i++) { }
                return html.Substring(loc + 27, i);
            }
        }

        public string GetGameTitleID()
        {
            return api.getGameTitleID();
        }

        public void Notify(string text)
        {
            api.Notify(text);
        }

        public byte[] ReadMemory(uint addr, uint size)
        {
            return api.ReadMemory(api.getCurrentPID(), addr, size);
        }

        public void ReleaseSubID(int id)
        {
            api.ReleaseSubID(id);
        }

        public int SubMemory(uint addr, uint size, Action<byte[]> callback)
        {
            return api.SubMemory(api.getCurrentPID(), addr, size, callback);
        }

        public void WriteMemory(uint addr, byte[] bytes)
        {
            api.WriteMemory(api.getCurrentPID(), addr, bytes);
        }
    }
}
