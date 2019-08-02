using System;
using System.Collections.Generic;
using System.Text;

namespace WMM.Data
{
    public class Category
    {
        public Category(string area, string name, CategoryType categoryType)
        {
            Area = area;
            Name = name;
            CategoryType = categoryType;
        }

        public string Area { get; }
        public string Name { get; }
        public CategoryType CategoryType { get; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object o)
        {
            if (o is Category c)
            {
                return c.Name == this.Name;
            }

            return false;
        }

        public static bool operator ==(Category c1, Category c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Category c1, Category oc2)
        {
            return !(c1 == oc2);
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
