using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ServerPongEmpty
{
    static class NTPTime
    {
        public static DateTime getNTPTime()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //IPAddress serverAddr = IPAddress.Parse("time.windows.com");

            IPAddress serverAddr = IPAddress.Parse("64.147.116.229");

            IPEndPoint endPoint = new IPEndPoint(serverAddr, 123);

            byte[] ntpData = new byte[48];

            ntpData[0] = 0x1B;
            ulong currSeconds = (ulong)((DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalSeconds);
            ulong currMillisecs = (ulong)((DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalMilliseconds);
            ulong tempSeconds;
            ntpData[40] = (byte)(currSeconds >> 24);
            tempSeconds = currSeconds << 40;
            ntpData[41] = (byte)(tempSeconds >> 56);
            tempSeconds = currSeconds << 48;
            ntpData[42] = (byte)(tempSeconds >> 56);
            tempSeconds = currSeconds << 56;
            ntpData[43] = (byte)(tempSeconds >> 56);

            currMillisecs = (ulong)(((UInt32)(currMillisecs / 1000)) * UInt32.MaxValue);

            ntpData[44] = (byte)(currMillisecs >> 24);
            tempSeconds = currMillisecs << 40;
            ntpData[45] = (byte)(tempSeconds >> 56);
            tempSeconds = currMillisecs << 48;
            ntpData[46] = (byte)(tempSeconds >> 56);
            tempSeconds = currMillisecs << 56;
            ntpData[47] = (byte)(tempSeconds >> 56);



            //Console.WriteLine("Current Seconds " + currSeconds);
            //Console.WriteLine("Trans Time Sending " + transTime1);

            // round trip delay = (T4 - T1) - (T2 - T3)



            sock.Connect(endPoint);
            DateTime T1 = DateTime.UtcNow;
            sock.Send(ntpData);
            while (sock.Receive(ntpData) < 44) { }
            DateTime T4 = DateTime.UtcNow;
            //UInt32 destTime = (UInt32)(ntpData[16] << 24) | (UInt32)(ntpData[17] << 16) | (UInt32)(ntpData[18] << 8) | (UInt32)(ntpData[19]);
            sock.Close();
            /*
            Console.WriteLine("LI " + (ntpData[0] >> 6));
            int temp = ntpData[0] << 2;
            temp = temp >> 5;
            Console.WriteLine("VN " + temp);
            temp = (byte)(ntpData[0] << 5);
            temp = temp >> 5;
            Console.WriteLine("Mode " + temp);
            Console.WriteLine("Stratum " + ntpData[1]);
            Console.WriteLine("Poll Interval " + ntpData[2]);
            Console.WriteLine("Precision " + ntpData[3]);
            */
            TimeSpan delay = T4.Subtract(T1);
            UInt32 refTime = (UInt32)(ntpData[16] << 24) | (UInt32)(ntpData[17] << 16) | (UInt32)(ntpData[18] << 8) | (UInt32)(ntpData[19]);
            UInt32 refTimeMicro = (UInt32)(ntpData[20] << 24) | (UInt32)(ntpData[21] << 16) | (UInt32)(ntpData[22] << 8) | (UInt32)(ntpData[23]);
            UInt32 refTimemilliseconds = (UInt32)(((double)refTimeMicro / UInt32.MaxValue) * 1000);

            UInt32 origTime = (UInt32)(ntpData[24] << 24) | (UInt32)(ntpData[25] << 16) | (UInt32)(ntpData[26] << 8) | (UInt32)(ntpData[27]);
            UInt32 origTimeMicro = (UInt32)(ntpData[28] << 24) | (UInt32)(ntpData[29] << 16) | (UInt32)(ntpData[30] << 8) | (UInt32)(ntpData[31]);
            UInt32 origTimemilliseconds = (UInt32)(((double)origTimeMicro / UInt32.MaxValue) * 1000);

            UInt32 recTime = (UInt32)(ntpData[32] << 24) | (UInt32)(ntpData[33] << 16) | (UInt32)(ntpData[34] << 8) | (UInt32)(ntpData[35]);
            UInt32 recTimeMicro = (UInt32)(ntpData[36] << 24) | (UInt32)(ntpData[37] << 16) | (UInt32)(ntpData[38] << 8) | (UInt32)(ntpData[39]);
            UInt32 recTimemilliseconds = (UInt32)(((double)recTimeMicro / UInt32.MaxValue) * 1000);

            UInt32 transTime = (UInt32)(ntpData[40] << 24) | (UInt32)(ntpData[41] << 16) | (UInt32)(ntpData[42] << 8) | (UInt32)(ntpData[43]);
            UInt32 transTimeMicro = (UInt32)(ntpData[44] << 24) | (UInt32)(ntpData[45] << 16) | (UInt32)(ntpData[46] << 8) | (UInt32)(ntpData[47]);
            UInt32 milliseconds = (UInt32)(((double)transTimeMicro / UInt32.MaxValue) * 1000);

            //Console.WriteLine(refTime);
            //Console.WriteLine(origTime);
            //Console.WriteLine(recTime);
            //Console.WriteLine(transTime);

            
            DateTime BaseDate = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime dt = BaseDate.AddSeconds(transTime).AddMilliseconds(milliseconds);

            //DateTime T2 = BaseDate.AddSeconds(recTime).AddMilliseconds(recTimemilliseconds);

            //delay = delay - (T2.Subtract(dt));

            //TimeSpan tsOffset = T2.Subtract(T1);
            //tsOffset = tsOffset.Add(dt.Subtract(T4));



            //uniClock.Start();
            

            return dt;
            
        }

    }
}
