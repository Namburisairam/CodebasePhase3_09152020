using System.Collections.Generic;

namespace EventManagement.Models
{
    public class FloorMapResponse
    {
        public List<FloorMapPin> Pin { get; set; }
        public Canvas canvas { get; set; }



        public class Coords
        {
            public string lat { get; set; }
            public string @long { get; set; }


            public Coords()
            {
                lat = "0";
                @long = "0";
            }
        }

        public class FloorMapPin
        {
            public string content { get; set; }

            public int PinID { get; set; }

            public List<int> Activities { get; set; }

            public List<int> sponsors { get; set; }

            public List<int> Exhibitors { get; set; }
            public Coords coords { get; set; }

            public FloorMapPin()
            {
                content = string.Empty;
                coords = new Coords();
            }
        }

        public class Canvas
        {
            public int? fmid { get; set; }
            public string src { get; set; }
            public string width { get; set; }
            public string height { get; set; }

            public Canvas()
            {
                fmid = 0;
                src = string.Empty;
                width = string.Empty;
                height = string.Empty;
            }
        }

        public FloorMapResponse()
        {
            Pin = new List<FloorMapPin>();
            canvas = new Canvas();

        }
    }
}