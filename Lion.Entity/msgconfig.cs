using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lion.Entity
{
    [Table("lt_msgconfig")]
    public class msgconfigEntity
    {
        [Key]
        public int id { get; set; }
        /// <summary>
        /// 通道商名字
        /// </summary>
        public string mer_name { get; set; }
        /// <summary>
        /// 区域
        /// </summary>
        public string area { get; set; }
        /// <summary>
        /// 提交地址
        /// </summary>
        public string url { get; set; }
        public string account { get; set; }
        public string passwd { get; set; }
        /// <summary>
        /// 激活状态
        /// </summary>
        public int active { get; set; }
        public int del { get; set; }
        public long times { get; set; }
    }
}
