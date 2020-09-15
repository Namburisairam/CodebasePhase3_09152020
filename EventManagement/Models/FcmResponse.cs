using System.Collections.Generic;

namespace EventManagement.Models
{
    public class FcmResponse
    {
        public long Multicast_id { get; set; }

        public int Success { get; set; }

        public int Failure { get; set; }

        public int Canonical_ids { get; set; }

        public List<Result> Results { get; set; }
    }

    public class Result
    {
        public string Error { get; set; }
    }
}