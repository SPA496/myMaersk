using System.Collections.Generic;
using myDamco.Database;

namespace myDamco.Areas.Administration.Models
{
    public class NewsModel
    {
        public IEnumerable<string> UAMApps;
        public IEnumerable<string> UAMFuncs;
        public IEnumerable<NewsCategory> Categories;
    }
}