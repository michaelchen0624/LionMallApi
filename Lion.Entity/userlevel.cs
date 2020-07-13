using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lion.Entity
{
    [Table("lt_userlevel")]
    public class userlevelEntity
    {
        [Key]
        public int id { get; set; }
        public int uid { get; set; }
        public int parent { get; set; }
        public long times { get; set; }
    }
    [Table("lt_levelextend")]
    public class levelextendEntity
    {
        [Key]
        public int id { get; set; }
        public int parent { get; set; }
        public int uid { get; set; }
        public int level { get; set; }
        public long times { get; set; }
    }
    [Table("lt_userlevelconfig")]
    public class userlevelconfigEntity
    {
        [Key]
        public int id { get; set; }
        public string level_name { get; set; }
        public string level_cnname { get; set; }
        public double total_amount { get; set; }
        /// <summary>
        /// 直接推荐层级
        /// </summary>
        public int direct_level { get; set; }
        /// <summary>
        /// 直接推荐对应层级的数量
        /// </summary>
        public int direct_count { get; set; }
        /// <summary>
        /// 可拿奖励级别 1=直推 2=直推+间接推荐
        /// </summary>
        public int reward_level { get; set; }
        public double weight { get; set; }
        public int bonus { get; set; }
        public double userbuy_max { get; set; }
        public double userbuy_min { get; set; }
    }
    public class levelInfo
    {
        public int uid { get; set; }
        public string usn { get; set; }
        public int level { get; set; }
        public double total_consum { get; set; }
    }
    public class levelCount
    {
        public int level { get; set; }
        public int count { get; set; }
    }
}
