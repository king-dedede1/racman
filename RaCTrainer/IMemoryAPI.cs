using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace racman
{
    public interface IMemoryAPI
    {
        string GetGameTitleID();

        string GetGameTitle();

        byte[] ReadMemory(uint addr, uint size);

        void WriteMemory(uint addr, byte[] bytes);

        int SubMemory(uint addr, uint size, Action<byte[]> callback);

        int FreezeMemory(uint addr, byte[] value);

        void ReleaseSubID(int id);

        void Notify(string text);

        void Disconnect();
    }
}
