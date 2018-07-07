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


namespace OpenDNS
{
    /// <summary>
    /// Response object as result of a dns query message. 
    /// Will be null unless query succesfull. 
    /// </summary>
    public class DnsResponse
    {
        private ResourceRecordCollection _ResourceRecords;

        //Read Only Public Properties
        public int QueryID { get; }

        public bool AuthorativeAnswer { get; }

        public bool IsTruncated { get; }

        public bool RecursionRequested { get; }

        public bool RecursionAvailable { get; }

        public ResponseCodes ResponseCode { get; }

        public ResourceRecordCollection Answers { get; }

        public ResourceRecordCollection Authorities { get; }

        public ResourceRecordCollection AdditionalRecords { get; }

        /// <summary>
        /// Unified collection of Resource Records from Answers, 
        /// Authorities and Additional. NOT IN REALTIME SYNC. 
        /// 
        /// </summary>
        public ResourceRecordCollection ResourceRecords
        {
            get
            {
                if (_ResourceRecords.Count == 0 && Answers.Count > 0 && Authorities.Count > 0 && AdditionalRecords.Count > 0)
                {
                    foreach (ResourceRecord rr in Answers)
                        _ResourceRecords.Add(rr);

                    foreach (ResourceRecord rr in Authorities)
                        _ResourceRecords.Add(rr);

                    foreach (ResourceRecord rr in AdditionalRecords)
                        _ResourceRecords.Add(rr);
                }

                return _ResourceRecords;
            }
        }

        public DnsResponse(int ID, bool AA, bool TC, bool RD, bool RA, int RC)
        {
            QueryID = ID;
            AuthorativeAnswer = AA;
            IsTruncated = TC;
            RecursionRequested = RD;
            RecursionAvailable = RA;
            ResponseCode = (ResponseCodes)RC;

            _ResourceRecords = new ResourceRecordCollection();
            Answers = new ResourceRecordCollection();
            Authorities = new ResourceRecordCollection();
            AdditionalRecords = new ResourceRecordCollection();
        }
    }
}
