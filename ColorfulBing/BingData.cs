using System;

public class BingData {
    public Data Data { get; set; }
    public Status Status { get; set; }
}

public class Data {
    public int Id { get; set; }
    public string Title { get; set; }
    public string Attribute { get; set; }
    public string Description { get; set; }
    public string Copyright { get; set; }
    public string Copyrightlink { get; set; }
    public string Startdate { get; set; }
    public string EndDate { get; set; }
    public string Fullstartdate { get; set; }
    public string Url { get; set; }
    public string Urlbase { get; set; }
    public string Hsh { get; set; }
    public string Qiniu_url { get; set; }
    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string Continent { get; set; }
    public string Thumbnail_pic { get; set; }
    public string Bmiddle_pic { get; set; }
    public string Original_pic { get; set; }
    public int Weibo { get; set; }
    public int Likes { get; set; }
    public int Views { get; set; }
    public int Downloads { get; set; }
}

public class Status {
    public int Code { get; set; }
    public string Message { get; set; }
}

