using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionGateWay
{
    public class RespData<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public T result { get; set; }
    }
    public class geneRetData
    {
        /// <summary>
        /// 生成返回类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_status"></param>
        /// <param name="_code"></param>
        /// <param name="t"></param>
        /// <param name="_msg"></param>
        /// <returns></returns>
        public static RespData<T> geneRate<T>(int _status, int _code, T t, string _msg = "")
        {
            return new RespData<T> { status = _status, code = _code, msg = _msg, result = t };
        }
        /// <summary>
        /// 生成返回类型扩展方法 _status 1时 正确的，大于 1 时错误码
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_status"></param>
        /// <param name="t"></param>
        /// <param name="_msg"></param>
        /// <returns></returns>
        public static RespData<T> geneRate<T>(int _status, T t, string _msg = "")
        {
            if (_status == 1)
                return new RespData<T> { status = _status, code = 200, msg = _msg, result = t };
            else
                return new RespData<T> { status = 0, code = _status, msg = _msg, result = t };
        }
    }
}
