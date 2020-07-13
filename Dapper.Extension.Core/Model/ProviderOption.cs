﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extension.Core
{
    public class ProviderOption
    {
        public ProviderOption(char openQuote, char closeQuote, char parameterPrefix)
        {
            OpenQuote = openQuote;
            CloseQuote = closeQuote;
            ParameterPrefix = parameterPrefix;
        }

        public char OpenQuote { get; set; }

        public char CloseQuote { get; set; }

        public char ParameterPrefix { get; set; }

        public string CombineFieldName(string field)
        {
            return OpenQuote + field + CloseQuote;
        }
    }
}
