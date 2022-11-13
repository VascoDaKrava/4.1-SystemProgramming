using System;


namespace SystemProgramming.Lesson3LLAPI
{
    [Serializable]
    public struct ConnectionPoint
    {
        public int HostID;
        public int ConnectionID;
        public int ChannelID;

        public override string ToString()
        {
            return $"Host {HostID}, Connection {ConnectionID}, Channel {ChannelID}";
        }

        public void Clear()
        {
            HostID = 0;
            ConnectionID = 0;
            ChannelID = 0;
        }

        //public int CompareTo(object obj)
        //{
        //    throw new NotImplementedException();
        //}
    }
}