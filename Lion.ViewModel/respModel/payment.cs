using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.ViewModel.respModel
{
    public class payMentView
    {
        public IList<payMentTitle> list;
    }
    public class payMentTitle
    {
        public string paytitle { get; set; }
        public string icon { get; set; }
        public string payname { get; set; }
        public string paysn { get; set; }
    }
    /// <summary>
    /// 用户收款方式界面
    /// </summary>
    public class upayMentView
    {
        public IList<upayMentTitle> list { get; set; }
    }
    /// <summary>
    /// 用户收款方式界面内容
    /// </summary>
    public class upayMentTitle
    { 
        public string paytitle { get; set; }
        public string payname { get; set; }
        public string icon { get; set; }
        public int active { get; set; }
        public string account { get; set; }
        public string paysn { get; set; }
    }
    public class addUPayMentView
    {
        public string title { get; set; }
        public string paysn { get; set; }
        public IList<addUPayContent> contentList { get; set; }
    }
    public class addUPayContent
    {
        public string title { get; set; }
        public string fieldtip { get; set; }
        public string submitTitle { get; set; }
        public int qrcode { get; set; }
        public int digFlag { get; set; }
        public int fieldsort { get; set; }
    }
    public class msgSign
    {
        public string sign { get; set; }
        public int expire { get; set; }
    }
}
