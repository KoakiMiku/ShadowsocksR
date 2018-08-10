/* 
 * Author: Ruy Delgado <ruydelgado@gmail.com>
 * Title: OpenDNS
 * Description: DNS Client Library 
 * Revision: 1.0
 * Last Modified: 2005.01.28
 * Created On: 2005.01.28
 * 
 * Note: Based on DnsLite by Jaimon Mathew
 * */

using System.Text;

namespace OpenDNS
{
    /// <summary>
    /// Base Resource Record class for objects returned in 
    /// answers, authorities and additional record DNS responses. 
    /// </summary>
    public class ResourceRecord
    {
        public string Name;
        public Types Type;
        public Classes Class;
        public int TimeToLive;
        public string RText;

        public ResourceRecord()
        {
        }

        public ResourceRecord(string _Name, Types _Type, Classes _Class, int _TimeToLive)
        {
            Name = _Name;
            Type = _Type;
            Class = _Class;
            TimeToLive = _TimeToLive;
        }

        public ResourceRecord(string _Name, Types _Type, Classes _Class, int _TimeToLive, string _RText)
        {
            Name = _Name;
            Type = _Type;
            Class = _Class;
            TimeToLive = _TimeToLive;
            RText = _RText;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Name=" + Name + "&Type=" + Type + "&Class=" + Class + "&TTL=" + TimeToLive);
            return sb.ToString();
        }
    }

}