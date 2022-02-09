using System.Collections.Generic;
using myDamco.Database;

namespace myDamco.Models
{
    public class NavigationModel
    {
        public List<Navigation> MenuItems;
        public Navigation activeItem;
        public string qsMenu;
    }
}