using System;
using System.Collections.Generic;
using System.Text;

namespace WMM.Data
{
    public class Goal
    {
        public Goal(Guid id, string name, string description, List<CategoryType> categoryTypeCriteria, List<string> areaCriteria)
        {
            Id = id;
            Name = name;
            Description = description;
            CategoryTypeCriteria = categoryTypeCriteria;
            AreaCriteria = areaCriteria;
        }

        public Guid Id { get; }
        public string Name { get; }
        public string Description { get; }
        public List<CategoryType> CategoryTypeCriteria { get; }
        public List<string> AreaCriteria { get; }
        public List<Category> CategoryCriteria { get; }
        public double LimitAmount { get; }
    }
}
