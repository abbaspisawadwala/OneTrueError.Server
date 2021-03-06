using System;
using OneTrueError.App.Modules.Similarities.Domain;

namespace OneTrueError.SqlServer.Modules.Similarities.Entities
{
    public class ContextCollectionPropertyValueDbEntity
    {
        public ContextCollectionPropertyValueDbEntity(SimilarityValue x)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            Value = x.Value;
            Count = x.Count;
            Percentage = x.Percentage;
        }

        protected ContextCollectionPropertyValueDbEntity()
        {
        }


        public int Count { get; set; }

        public int Percentage { get; set; }
        public string Value { get; set; }
    }
}