using System;
using System.Collections.Generic;
using System.Text;

namespace WordShuffle
{
    public class Metadata
    {
        public string uri { get; set; }
        public string type { get; set; }
    }

    public class TumbnailMetadata
    {
        public string type { get; set; }
    }

    public class Thumbnail
    {
        public TumbnailMetadata __metadata { get; set; }
        public string MediaUrl { get; set; }
        public string ContentType { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string FileSize { get; set; }
    }

    public class Result
    {
        public Metadata __metadata { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public string MediaUrl { get; set; }
        public string SourceUrl { get; set; }
        public string DisplayUrl { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string FileSize { get; set; }
        public string ContentType { get; set; }
        public Thumbnail Thumbnail { get; set; }
    }

    public class D
    {
        public List<Result> results { get; set; }
        public string __next { get; set; }
    }

    public class ImageSearchObject
    {
        public D d { get; set; }
    }
}
