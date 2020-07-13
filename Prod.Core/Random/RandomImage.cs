﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Prod.Core
{
    public class RandomImage
    {
        private string _contenttype;
        private byte[] _image;

        /// <summary>
        /// 图片输出类型
        /// </summary>
        public string ContentType
        {
            get { return _contenttype; }
            set { _contenttype = value; }
        }

        /// <summary>
        /// 图片
        /// </summary>
        public byte[] Image
        {
            get { return _image; }
            set { _image = value; }
        }
    }
}
