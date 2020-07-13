using System;
using System.Collections.Generic;
using System.Text;

namespace DataCommon
{
    public class RepositoryFactory
    {
        private static IRepository repository;
        static RepositoryFactory()
        {
            repository = new RepositoryBase();
        }
        public static IRepository GetRepository()
        {
            return repository;
        }
        public static IRepository Repository
        {
            get { return repository; }
        }
    }
}
