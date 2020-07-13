using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.ViewModel.respModel
{
    public class userSN
    {
        public string uid { get; set; }
    }
    public class userInfo
    {
        public string userName { get; set; }
        public string phone { get; set; }
        public string phonearea { get; set; }
        public string fullname { get; set; }
        public string userSN { get; set; }
    }
    public class userCenterViewModel
    {
        public string userName { get; set; }
        public string iconUrl { get; set; }
        public string total_balance { get; set; }
        public string shopping_balance { get; set; }
        public string total_consume { get; set; }
        public int userLevel { get; set; }
        public IList<urlInfo> urlList { get; set; }
    }
    public class respUserName
    {
        public string userName { get; set; }
    }
    public class inviteViewModel
    {
        public int direct_count { get; set; }
        public int indirect_count { get; set; }
        public string parent { get; set; }
        public string inviteCode { get; set; }
        public IList<directSubItem> subList { get; set; }
    }
    public class directSubItem
    {
        public string userName { get; set; }
        public int level { get; set; }
        public string headImg { get; set; }
        public string phonearea { get; set; }
        public string phone { get; set; }
    }
    public class respInviteUrl
    {
        public string qrCode { get; set; }
    }
    public class urlInfo
    {
        public string name { get; set; }
        public string url { get; set; }
    }
    public class imgAuth
    {
        public string Authorization { get; set; }
    }
}
