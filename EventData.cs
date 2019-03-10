using System;
using System.Runtime.Serialization;

namespace TCPTest
{
    [DataContract(Name = "EVENT")]
    public class EventContent
    {
        [DataMember(Name = "CUSTID")]
        public int CustId { get; set; }

        [DataMember(Name = "DEVICENO")]
        public string DeviceNo { get; set; }

        [DataMember(Name = "EVENTCODE")]
        public string EventCode { get; set; }

        [DataMember(Name = "EVENTTIME")]
        public DateTime EventTime { get; set; }

        [DataMember(Name = "LATITUDE")]
        public double Latitude { get; set; }

        [DataMember(Name = "LONGITUDE")]
        public double Longitude { get; set; }

        [DataMember(Name = "MESSAGE")]
        public string Message { get; set; }

        [DataMember(Name = "OBNUMBER")]
        public string[] ObNumber { get; set; }

        [DataMember(Name = "PARTITION")]
        public string Partition { get; set; }

        [DataMember(Name = "FORMAT")]
        public string PID { get; set; }

        [DataMember(Name = "RECEIVER")]
        public string Receiver { get; set; }

        [DataMember(Name = "REFNO")]
        public string RefNo { get; set; }

        [DataMember(Name = "RSSI")]
        public string Rssi { get; set; }

        [DataMember(Name = "SEQ")]
        public int Seq { get; set; }

        [DataMember(Name = "USERID")]
        public string UserId { get; set; }

        [DataMember(Name = "ZONE")]
        public int Zone { get; set; }

        [DataMember(Name = "ZONEDESC")]
        public string ZoneDesc { get; set; }
    }

    [DataContract(Name = "EVENT")]
    public class EventData
    {
        public EventData()
        {
            Event = new EventContent();
        }

        [DataMember(Name = "EVENT")]
        public EventContent Event { get; set; }
    }
}