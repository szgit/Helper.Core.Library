using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.IQuery
{
    public class EntityQueryable<T> : IQueryable<T>
    {
        public EntityQueryable()
        {
            this.Provider = new EntityProvider();
            this.Expression = Expression.Constant(this);
        }

        public EntityQueryable(EntityProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (this.Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Expression Expression { get; set; }

        public IQueryProvider Provider { get; set; }

        public string ToSql()
        {
            return "";
        }
    }
}
