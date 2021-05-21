using System;
using System.Collections.Generic;

namespace RgDevBot.ObjectModel
{
    public class PageInfo
    {
        public string endCursor { get; set; }
        public bool hasNextPage { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string title { get; set; }
        public string slug { get; set; }
        public string outerSite { get; set; }
    }

    public class Node
    {
        public string id { get; set; }
        public string slug { get; set; }
        public DateTime date { get; set; }
        public string title { get; set; }
        public string shortDescription { get; set; }
        public List<Project> projects { get; set; }
        public object imagePageNewsDisplay { get; set; }
        public object imagePageNewsPreview { get; set; }
        public object imageDisplay { get; set; }
        public object imagePreview { get; set; }
    }

    public class Edge
    {
        public Node node { get; set; }
    }

    public class AllPosts
    {
        public PageInfo pageInfo { get; set; }
        public int totalCount { get; set; }
        public List<Edge> edges { get; set; }
    }

    public class Data
    {
        public AllPosts allPosts { get; set; }
    }

    public class Root
    {
        public Data data { get; set; }
    }
}